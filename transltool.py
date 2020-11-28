import argparse
import json
import os
import shutil
import sys

def dumpDatabase(database_path, output_path):
    # open and parse database JSON
    with open(database_path, encoding="utf8") as f:
        data = json.load(f)
        
    # delete old dump directory
    shutil.rmtree(output_path, ignore_errors=True)
    
    # dump conversations
    for conversation in data["conversations"]:
        # generate human-readable filename
        filename = ''.join(a for a in conversation["title"].title() if a.isalnum())
        filename = filename[0].lower() + filename[1:] + ".transl"
        filename = conversation["type"] + "/" + filename
            
        # dump dialogue-less orbs into one file
        append = False
        if conversation["type"] == "orb" and len(conversation["entries"]) == 0:
            filename = "orb/orbs.transl"
            append = True
        
        # dump to file
        dumpConversation(conversation, os.path.join(output_path, filename), append)
        
    # dump misc
    for category in data["miscellaneous"]:
        # get filename and output path
        filename = category + ".transl"
        filepath = os.path.join(output_path, filename)
        
        # write to file
        entries = data["miscellaneous"][category]
        with open(filepath, "a", encoding="utf8") as f:
            for key in entries:
                f.write("{}: # {}\n".format(key, entries[key].replace("\n", "\\\n")))
        
def dumpConversation(conversation, path, append):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "a" if append else "w", encoding="utf8") as f:
        # dump metadata
        f.write("# CONVERSATION METADATA\n")
        for key in conversation["metadata"]:
            f.write("{}: # {}\n".format(key, conversation["metadata"][key].replace("\n", "\\\n")))
            
        # skip entries if there are none
        if len(conversation["entries"]) == 0:
            return
        
        # dump conversation entries
        f.write("\n# CONVERSATION ENTRIES\n")
        for entry in conversation["entries"]:
            f.write("{}: # {} {}\n".format(entry["id"], entry["actor"].upper(), entry["text"].replace("\n", "\\\n")))

def concatFiles():
    pass

# define command line arguments
arg_parser = argparse.ArgumentParser(description="Transltool - resource \
management utility for Disco Translator 2")

actiongroup = arg_parser.add_mutually_exclusive_group(required=True)
actiongroup.add_argument("--dump", type=str, metavar="PATH",
    help="Split a JSON database into a .transl file hierarchy")
actiongroup.add_argument("--concat", type=str, metavar="PATH",
    help="Concatenate a .transl file hierarchy into a single file")
arg_parser.add_argument("--output", type=str, metavar="PATH", default=".",
    help="Output path for the above options")

# show help if there are no arguments
if len(sys.argv) < 2:
    arg_parser.print_help()
    sys.exit(1)

# perform the requested action
args = arg_parser.parse_args()
if args.dump:
    dumpDatabase(args.dump, args.output)
elif args.concat:
    concatFiles(args.concat, args.output)
