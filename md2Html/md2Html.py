from pathlib import Path
from shutil import copyfile

import mistune

DEST_DIR = "../build/doc/"

template = open("template.html", "r").read()
files = ["../README.md", "../virtualKeyNames.md"]
for mdPath in files:
    mdFilename = Path(mdPath).name
    file = open(mdPath, "r")
    md = file.read()
    html = mistune.markdown(md)
    html = template.replace("${body}", html).replace("${title}", mdFilename.replace(".md", ""))
    # also replace local markdown links with local html links
    html = html.replace(".md", ".html")
    htmlFilename = DEST_DIR + mdFilename.replace(".md", ".html")
    parentDir = Path(htmlFilename).parent
    parentDir.mkdir(parents=True, exist_ok=True)
    with open(htmlFilename, "w") as htmlFile:
        htmlFile.write(html)
#   don't forget to copy the css file
copyfile("html.css", DEST_DIR + "/html.css")
