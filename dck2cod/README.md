# dck2cod

Converts decks saved in Shandalar's proprietary `.dck` format to Cockatrice's `.cod` format.

## Example

Here is an example .dck file

```
Lord of Fate (Bl/Wh, 4th Edition)

.188	11	Plains
.239	11	Swamp
.55	2	Dark Ritual
.28	3	Castle
.108	3	Healing Salve
.112	3	Holy Strength
.259	3	Unholy Strength
.550	4	Yotian Soldiers
.221	4	Serra Angel
.736	3	Osai Vultures
.182	3	Pestilence
.871	2	Necropolis of Azaar
.70	3	Drudge Skeletons
.616	2	Divine Transformation

.vNone
.514	3	Ornithopter
.vBlack
.930	3	Brass Man
.vBlue
.514	3	Ornithopter
.vGreen
.3	3	Animate Dead
.vRed
.466	3	Amulet of Kroog
.vWhite
.930	3	Brass Man
```

`dck2cod` can convert it to this

```
<?xml version="1.0" encoding="utf-8"?>
<cockatrice_deck version="1">
  <deckname>Lord of Fate</deckname>
  <comments></comments>
  <zone name="main">
    <card number="11" name="Plains" />
    <card number="11" name="Swamp" />
    <card number="2" name="Dark Ritual" />
    <card number="3" name="Castle" />
    <card number="3" name="Healing Salve" />
    <card number="3" name="Holy Strength" />
    <card number="3" name="Unholy Strength" />
    <card number="4" name="Yotian Soldier" />
    <card number="4" name="Serra Angel" />
    <card number="3" name="Osai Vultures" />
    <card number="3" name="Pestilence" />
    <card number="2" name="Necropolis of Azaar" />
    <card number="3" name="Drudge Skeletons" />
    <card number="2" name="Divine Transformation" />
    <card number="3" name="Ornithopter" />
  </zone>
  <zone name="side">
    <card number="3" name="Brass Man" />
    <card number="3" name="Animate Dead" />
    <card number="3" name="Amulet of Kroog" />
  </zone>
</cockatrice_deck>
```

## Issues

* Source and output paths are hardcoded.

___

# dck files

Here are some interesting facts about .dck files that I have been able to deduce, or suspect to be true.

* The files have padded integer names like `0012.dck`
* Each file has the following format

  ```
  <name> <description (optional)>

  <card line>
  <card line>
  <card line>
  ...

  <sideboard section (optional)>
  ```

* `<name>` is the name of the character that wields the deck. (ex. `Lord of Fate`)
* `<description>` is present on most decks and typically includes
  * non-standarized color abbreviations (ex. `R` or `Re` for red)
  * non-standardized set names/abbreviations (ex. `4th` or `4th Edition`, never `4E` or `4ED`)
* Each `<card line>` has the format `.<id> <count> <name>`. (ex. `.221 2 Serra Angel`)
  * `<id>` appears to be a card ID, because cards have the same IDs when used in different decks. (ex. `Lightning Bolt` is always with `145`)
  * `<count>` appears to be a count, because it is never greater than 4, except for basic lands.
  * `<name>` may only be for human readability. A few cards are mispelled consistently across different files, but Will-'o-the-Wisp is spelled multiple ways in different files. The presence of ID's supports this hypothesis.

## Sideboards

`<sideboard section>` is a little unintuitive as a human Magic player, but it makes sense when you think about programming the game's AI.

It doesn't represent a sideboard like you would in a typical deck list for humans. Instead it includes the sideboard cards, the cards in the main deck to sub out for sideboard cards, and basic instructions for when to make substitutions.

`<sideboard section>` is structured like this

```
.vNone
<card line> (0 or more)
.vBlack
<card line> (0 or more)
.vBlue
<card line> (0 or more)
.vRed
<card line> (0 or more)
.vWhite
<card line> (0 or more)
```

There are 6 sub-sections (`None`, `Black`, `Blue`, `Green`, `Red`, `White`), which typically have 1-3 card lines each. The total count of cards (not lines) in each section is always the same.

* Ex. If `None` has 3 Giant Growths, `Blue` may have 3 Web or (2 Web, 1 Regeneration), but will never have more or less than 3 total cards.

I have not extensively tested to see how the AI decides to make sideboard substitutions, but intuitively it seems the logic is this:

* The NPC's default deck is all the cards before the sideboard section, plus all the cards in the `None` sideboard sub-section.
* If the NPC is playing against a primarily black deck, switch all the cards in the `None` sub-section with the `Black` sub-section. (And so on, for the other colors...)

In many cases, some of the sideboard sub-sections are identical to the `None` sub-section or other sub-sections. For example, with Lord of Fate:

```
.vNone
.514	3	Ornithopter
.vBlack
.930	3	Brass Man
.vBlue
.514	3	Ornithopter
.vGreen
.3	3	Animate Dead
.vRed
.466	3	Amulet of Kroog
.vWhite
.930	3	Brass Man
```

The AI logic would be

* Default to having 3 Ornithopter in the deck
* Swap for 3 Brass Man if playing against Black or White
* Swap for 3 Animate Dead if playing against Green
* Swap for 3 Amulet of Kroog if playing against Red
* No swaps if playing against Blue

And written as a typical sideboard list:
```
3 Brass Man
3 Animate Dead
3 Amulet of Kroog
```
