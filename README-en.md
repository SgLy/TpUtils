## Tp Utils

A plugin for [TShock](https://github.com/Pryaxis/TShock) to provide some extra tp commands.

### All available commands

- `tpd`

    Teleport to your last death position.

    Possible results:
    - success: `Teleported to: {X}, {Y}`
    - failed: `No recorded death position yet.`

- `add <name>`

    Add current position to favourite positions, name is for hinting only.

    Possible results:
    - success: `Labeled <{id}> ({X}, {Y}) {name}`
    - failed: `Invalid syntax, usage: add <name>`

- `ls`

    List all favourited positions.

    Possible results:
    - success: 
        ```
        All labeled positions:
          <{id}> ({X}, {Y}) {name}
        Total: {Count} labels
        ```

- `tpl <id>`

    Teleport to a favourite position, identified by Id.

    Possible results:
    - success: `Teleported to: <{id}> ({X}, {Y}) {name}`
    - failed: `Invalid syntax, usage: tpl <id>`
    - failed: `{id} is not a valid id, id should be an int`
    - failed: `{id} is not a valid id, id should be between 0-{Count - 1}`

- `rm <id>`

    Remove a favourite position, identified by Id.

    Possible results:
    - success: `Removed <{id}> ({X}, {Y}) {name}`
    - failed: `Invalid syntax, usage: rm <id>`
    - failed: `{id} is not a valid id, id should be an int`
    - failed: `{id} is not a valid id, id should be between 0-{Count - 1}`
