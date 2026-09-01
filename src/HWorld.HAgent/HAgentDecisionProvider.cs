using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using HWorld.Core.World;

namespace HWorld.HAgent
{
    /// <summary>
    /// Adapts a long-lived HAgent runtime instance to HWorld's generic decision-provider contract.
    /// HWorld remains authoritative over action meaning and world-side validation.
    /// </summary>
    public sealed class HAgentDecisionProvider : IWorldActorDecisionProvider, IDisposable
    {
        private const string ActionSchema =
            "{\"type\":\"object\",\"properties\":{" +
            "\"action\":{\"type\":\"string\",\"enum\":[\"move\",\"turn\",\"wait\"]}," +
            "\"directionX\":{\"type\":\"number\"}," +
            "\"directionY\":{\"type\":\"number\"}," +
            "\"durationSeconds\":{\"type\":\"number\",\"exclusiveMinimum\":0}," +
            "\"angleDegrees\":{\"type\":\"number\"}" +
            "},\"required\":[\"action\"],\"additionalProperties\":false}";

        private readonly HAgentClient _client;
        private readonly AgentRuntimeInstance _runtimeInstance;
        private bool _disposed;

        public HAgentDecisionProvider(HAgentClient client, AgentRuntimeInstance runtimeInstance)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _runtimeInstance = runtimeInstance ?? throw new ArgumentNullException(nameof(runtimeInstance));
        }

        public AgentRuntimeInstance RuntimeInstance
        {
            get { return _runtimeInstance; }
        }

        public async Task<WorldActorAction> DecideAsync(
            WorldActorDecisionContext context,
            CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HAgentDecisionProvider));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = new AgentExecutionRequest
            {
                AgentId = _runtimeInstance.ProfileId,
                Messages = new List<AIMessage>
                {
                    new AIMessage("user", BuildDecisionPrompt(context))
                }.AsReadOnly(),
                HostCorrelationId = Guid.NewGuid().ToString("N"),
                HostContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["actor_id"] = context.ActorId.ToString("N"),
                    ["simulation_time"] = context.SimulationTime.ToString("R", CultureInfo.InvariantCulture)
                },
                StructuredOutput = new StructuredOutputOptions
                {
                    SchemaJson = ActionSchema,
                    RequireValidJson = true
                },
                Options = new AgentExecutionOptions()
            };

            var execution = await _client.ExecuteAsync(
                _runtimeInstance,
                request,
                cancellationToken).ConfigureAwait(false);

            if (execution == null || execution.Response == null)
                throw new InvalidOperationException("HAgent returned no execution response.");

            return ParseAction(execution.Response.StructuredOutputJson);
        }

        public void Shutdown()
        {
            _runtimeInstance.Shutdown();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _runtimeInstance.Shutdown();
        }

        private static string BuildDecisionPrompt(WorldActorDecisionContext context)
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Choose the next action for the actor from its current observation.",
                "Return exactly one JSON object matching the structured-output schema. Do not explain the decision.",
                "",
                "Actor:",
                "  id: " + context.ActorId.ToString("N"),
                "  position: (" + Format(context.Position.X) + ", " + Format(context.Position.Y) + ")",
                "  rotationDegrees: " + Format(context.RotationDegrees),
                "  width: " + Format(context.Width),
                "  height: " + Format(context.Height),
                "  speed: " + Format(context.Speed),
                "  simulationTime: " + Format(context.SimulationTime),
                "",
                "Observation:",
                context.Observation ?? string.Empty
            });
        }

        private static WorldActorAction ParseAction(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("HAgent returned empty structured output.");

            using (var document = JsonDocument.Parse(json))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException("HAgent structured output must be a JSON object.");

                var root = document.RootElement;
                string action;
                if (!TryGetString(root, "action", out action))
                    throw new InvalidOperationException("HAgent structured output is missing 'action'.");

                switch (action.Trim().ToLowerInvariant())
                {
                    case "move":
                    {
                        var directionX = GetRequiredNumber(root, "directionX");
                        var directionY = GetRequiredNumber(root, "directionY");
                        var duration = GetRequiredPositiveNumber(root, "durationSeconds");
                        return new WorldActorAction(WorldActorActionKind.Move, directionX, directionY, duration);
                    }

                    case "turn":
                    {
                        var angle = GetRequiredNumber(root, "angleDegrees");
                        return new WorldActorAction(WorldActorActionKind.Turn, angle, 0d, 0d);
                    }

                    case "wait":
                    {
                        var duration = GetRequiredPositiveNumber(root, "durationSeconds");
                        return new WorldActorAction(WorldActorActionKind.Wait, 0d, 0d, duration);
                    }

                    default:
                        throw new InvalidOperationException("Unsupported HWorld action returned by HAgent: " + action);
                }
            }
        }

        private static string Format(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static bool TryGetString(JsonElement root, string propertyName, out string value)
        {
            JsonElement element;
            if (!root.TryGetProperty(propertyName, out element) || element.ValueKind != JsonValueKind.String)
            {
                value = string.Empty;
                return false;
            }

            value = element.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static double GetRequiredNumber(JsonElement root, string propertyName)
        {
            JsonElement element;
            if (!root.TryGetProperty(propertyName, out element) || element.ValueKind != JsonValueKind.Number)
                throw new InvalidOperationException("HAgent structured output is missing numeric '" + propertyName + "'.");

            double value;
            if (!element.TryGetDouble(out value) || double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidOperationException("HAgent returned an invalid numeric value for '" + propertyName + "'.");

            return value;
        }

        private static double GetRequiredPositiveNumber(JsonElement root, string propertyName)
        {
            var value = GetRequiredNumber(root, propertyName);
            if (value <= 0d)
                throw new InvalidOperationException("HAgent returned a non-positive value for '" + propertyName + "'.");
            return value;
        }
    }
}
