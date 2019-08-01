# CSVStringResourceConverter

Convert .csv or .tsv file to string resource files for Android and iOS applications.

.tsv extension is recommended.

## Usage

We need exported excel file like a .tsv which using for convert.
```
drag-and-drop exported file into CSV2MobileResource.exe
```

then program will make new folder in current directory with output files.

## Hot to write resources

I'm using google spreadsheet to manage string resources. 

but you can use anythings as long as the result is .csv or .tsv.

#### Examples
- [Google spreadsheet](https://docs.google.com/spreadsheets/d/1z0b0o6j4zkVgq9PvMDPeHzoQfUg8EmupAZfqQ61u6-8/edit?usp=sharing)
- [Exported tsv](https://raw.githubusercontent.com/SasarinoMARi/CSV2MobileResource/files/CSV%20Converter%20Example.tsv)

sheet need specific columns...

### Essential columns
- **screen / key** : this will be resource's id
- **value** : this will be resource's value.
- **translatable** : this meaning **value** can translatable string
- **uses-in-somewhere** : if you developing similar projects and sharing part of string resource, you can naming column which startwiths "uses-in-", then converter has divide output by flavor
- value-blabla : if you need localization strings, you can naming column which startwiths "value-"

