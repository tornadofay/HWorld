# Console Renderer

The console is a real renderer and development environment, not merely logging.

## Initial mode

Use a character grid:

```text
................................
.............A..................
.............#####..............
..........................X.....
................................
```

## Upgrade path

The console renderer can later add:

- ANSI colors
- Unicode block characters
- multiple visual layers
- camera viewport
- status panels
- agent statistics
- live event feed
- scrolling
- configurable refresh rate

This makes the same simulation usable without WinForms and useful for headless or low-resource experimentation.
