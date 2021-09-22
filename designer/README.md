# designer

Designer is a CLI for automating the custom Magic card site [mtg.design](mtg.design).

## Use cases

* Downloading card data as a backup.
* Generating a file that can be used to print card sheets.
* Auditing a set of cards for typos, missing fields, or other quality issues.
* Automatically assinging collectors numbers to each card in a set.

## ⚠️ A note of caution

You could easily use this to delete all the cards you've spent a lot of time working on.

* Test the tool on cards you don't care about first.
* Run `set pull` to create a local backup of a set before doing any kind of update operation like `set delete` or `set scrub`.

You could also use this to spam mtg.design with a lot of API requests.

* Please be courteous, don't run it in a loop overnight, etc.

## Usage

### Pre-requisites

Create an account at [mtg.design](mtg.design).

### General help

To see details of available commands:
```
mtgd --help
```
Output:
```
USAGE: mtgd [--help] [<subcommand> [<options>]]

SUBCOMMANDS:

    workspace <options>   Gets or sets workspace directory for later requests
    login <options>       Authenticates and saves cookie for later requests
    set <options>         Performs operations on sets of cards.
    card <options>        Performs operations on individual cards.

    Use 'mtgd <subcommand> --help' for additional information.

OPTIONS:

    --help                display this list of options.
```

A few general rules:

* Most things are not case sensitive (commands, arguments, paths, card names, set abbreviations).
* Most named arguments have a one letter shorthand (ex. `--email` and `-e`)

Create a workspace

Your workspace is the folder where all data files related to designer will go.
Before you can run any other commands, you need to set the workspace folder. (The folder does not need to exist yet.)
```
mtgd workspace C:\Users\N.Bolas\Documents\MyWorkspace
```

You can also use environment variables in paths.
```
mtgd workspace %USERPROFILE%\Documents\MyWorkspace
```

If you need spaces, use quotes.
```
mtgd workspace "C:\My Documents\This Is My Workspace"
```

If you forget what you set the workspace to, you can check.
```
mtgd workspace
Workspace is currently set to C:\Users\N.Bolas\Documents\MyWorkspace.
```

### Login

Now that you have a workspace, login. This will save a cookie in the workspace folder for use on later commands, and can optionally save your credentials.
```
mtgd login --email n.bolas@test.com --pass abc123
```

You can also save your credentials so you don't have to type them out again when your cookie expires.

⚠️ Saved credentials are not currently encrypted on disk.

```
mtgd login -e n.bolas@test.com -p abc123 --saveCreds
```

When you next login:
```
mtgd login
```

### Use cases

To download all card data and images:
```
mtgd set pull ABC
```

To automatically assign collectors numbers:
```
mtgd set scrub ABC
```

To rename a set:
```
mtgd set rename --from OLD --to NEW
```

To copy a set (so you can edit non-destructively):
```
mtgd set copy -f OLD -t NEW
```

To copy a single card:
```
mtgd card copy -f OLD -t NEW -n "Infernal Spawn of Evil"
```

To delete a card (⚠️):
```
mtgd card delete --set ABC --name Werebear
```

To delete a set (⚠️⚠️⚠️):
```
mtgd set delete ABC
```

###

## Workspace structure

Most of the files in the workspace will be generated by the app. The ones that must be manually edited have `***` next to their names.

```
C:\Users\N.Bolas\Documents\MyWorkspace
|- ST1
   |- *** center-fixes.json ***
   |- details.json
   |- layout.html
   |- card1.jpg
   |- card2.jpg
   |- ...
|- ST2
   |- ...
|- cookie.json
|- credentials.json
```

The top-level of the workspace has folders for each card set, and a few files used for authentication.

* `cookie.json` stores a cookie to use on later requests

  This is created by `login`

* `credentials.json` stores user credentials for later login requests.

  This is created if you use `login -e <email> -p <pass> --savecreds`

* Set directories (`ST1`, `ST2`) will each contain all the files for one card set. The names are the set abbreviations that will appear in the bottom-left corner of cards.

Within a set directory

* `center-fixes.json` is to compensate for a bug in mtg.design, where if you save a card with centered text, when you view that card later it says the text is not centered. This file is optional, but necessary if you want to preserve centering when updating cards. It must be created by hand (for now) and should be structured as a JSON array of card names. Ex:

  ```json
  [
    "Lightning Bolt",
    "Dark Ritual"
  ]
  ```

* `details.json` is generated when you run `set pull` and contains all data for each card in the set

* `layout.html` is generated when you run `set layout` and can be used to create print sheets. Conversion to PDF in a browser is still (for now) required to make printing practical.

* Card image files are also saved when you run `set pull`. These will have the card name with some characters escaped. Ex:

  ```
  Lightning Bolt -> lightning-bolt.jpg
  ```