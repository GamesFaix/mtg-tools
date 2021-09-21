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
