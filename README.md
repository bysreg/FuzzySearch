# FuzzySearch
VS 2017 extension

This works with VS2017 and CMake open folder project.  
Fuzzy Search defaults to search every files in the workspace folder.  
To only search for specific subfolders, list them separated by new line in ".fuzzysearchsettings" in your workspace folder.  
Close the Fuzzy Search window using "Tab".  
Select the suggestion using arrow key up and down and pressing enter on the suggestion

# Todo
- Do exhaustive search for fuzzy search
- consider camel case for heuristic (just like separator)
- Able to close window using escape key
- Pressing up and down should scroll the suggestion list
- bold the letters that match with the search query