from pathlib import Path
from shutil import copyfile
import os

import mistune

scriptDir = os.path.dirname(os.path.abspath(__file__))
DEST_DIR = "{}/../build/doc/".format(scriptDir)
print("{}".format(scriptDir))

template = open("{}/template.html".format(scriptDir), "r").read()
files = ["{}/../README.md".format(scriptDir), "{}/../virtualKeyNames.md".format(scriptDir)]
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
copyfile("{}/html.css".format(scriptDir), DEST_DIR + "/html.css")
