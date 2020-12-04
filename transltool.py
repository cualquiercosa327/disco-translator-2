import argparse
import json
import os
import shutil
import sys

def dumpDatabase(database_path, output_path, indented):
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
        dumpConversation(conversation, os.path.join(output_path, filename), append, indented)
        
    # dump misc
    for category in data["miscellaneous"]:
        # get filename and output path
        filename = category + ".transl"
        filepath = os.path.join(output_path, filename)
        
        # write to file
        entries = data["miscellaneous"][category]
        with open(filepath, "a", encoding="utf8") as f:
            for key in entries:
                f.write("{}: # {}\n".format(key, entries[key].replace("\n", "\\\n#")))
                
def buildConversationTree(conversation):
    # resolve entries dictionary
    entries = conversation["entries"]
    
    # add additional fields to entries
    for entry in entries.values():
        entry["in_reply_to"] = []
        entry["assigned_number"] = -1
        entry["indent"] = 0
        entry["visited"] = False
        
    # build reply graph
    for entry_id, entry in entries.items():
        for lead in entry["leadsTo"]:
            # skip exit leads
            if lead in entries:
                entries[lead]["in_reply_to"] += [entry_id]
                
    # ordered entries to be printed
    output = []
    
    # add roots and their branches
    root_number = 0
    for root in conversation["roots"]:
        root_number = buildBranch(root, root_number, 0, entries, output)
        
    # resolve reply ids
    for entry in output:
        resolved_ids = sorted([entries[id]["assigned_number"] for id in entry["in_reply_to"]])
        resolved_ids = [id for id in resolved_ids if id < entry["assigned_number"]]
        resolved_ids = [str(id) for id in resolved_ids]
        resolved_ids = ", ".join(resolved_ids)
        
        entry["in_reply_to_resolved"] = resolved_ids or None
        
    return output
    
def buildBranch(root, root_number, indent, entries, output):
    # add current node to output if it's a top root
    if indent == 0:
        entries[root]["assigned_number"] = root_number
        entries[root]["indent"] = indent
        entries[root]["visited"] = True
        output += [entries[root]]
        indent += 1
        root_number += 1
        
    # add children to output
    added_children = []
    for lead in entries[root]["leadsTo"]:
        # skip previously visited nodes and exit nodes
        if lead not in entries or entries[lead]["visited"]:
            continue
        
        # register child
        entries[lead]["assigned_number"] = root_number
        entries[lead]["indent"] = indent
        entries[lead]["visited"] = True
        output += [entries[lead]]
        added_children += [lead]
        root_number += 1
    
    # add grandchildren
    for child in added_children:
        root_number = buildBranch(child, root_number, indent + 1, entries, output)
    
    # return next available number
    return root_number
        
def dumpConversation(conversation, path, append, indented):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "a" if append else "w", encoding="utf8") as f:
        # dump metadata
        f.write("# CONVERSATION METADATA\n")
        for key in conversation["metadata"]:
            f.write("{}: # {}\n".format(key, conversation["metadata"][key].replace("\n", "\\\n#")))
            
        # skip entries if there are none
        if len(conversation["entries"]) == 0:
            return
        
        # dump conversation entries
        f.write("\n# CONVERSATION ENTRIES\n")
        
        # construct conversation tree
        ordered_entries = buildConversationTree(conversation)
        
        # print ordered entries
        for entry in ordered_entries:
            indent = "\t" * entry["indent"]
            number = entry["assigned_number"]
            actor = entry["actor"].upper()
            reply_info = ""
            
            # format reply info
            reply_format = " (to {})" if indented else ", in reply to {}"
            if entry["in_reply_to_resolved"] is not None:
                reply_info = reply_format.format(entry["in_reply_to_resolved"])
            
            # format entry
            for fieldid, fieldtext in entry["fields"].items():
                fieldtext = fieldtext.replace("\n", "\\\n#")
                
                if indented:
                    f.write("{}: # ({})\t{}{}: {}{}\n".format(fieldid, number,
                    indent, actor, fieldtext, reply_info))
                else:
                    f.write("{}: # ({}{}) {}: {}\n".format(fieldid, number,
                    reply_info, actor, fieldtext))
def concatFiles():
    pass

# define command line arguments
arg_parser = argparse.ArgumentParser(description="Transltool - resource \
management utility for Disco Translator 2")

actiongroup = arg_parser.add_mutually_exclusive_group(required=True)
actiongroup.add_argument("--dump", type=str, metavar="PATH",
    help="split a JSON database into a .transl file hierarchy")
actiongroup.add_argument("--concat", type=str, metavar="PATH",
    help="concatenate a .transl file hierarchy into a single file")
arg_parser.add_argument("--indented", default=False, action='store_true',
    help="use an indented format when dumping dialogues")
arg_parser.add_argument("output", type=str, metavar="OUTPUT_PATH", default=".",
    help="output path for the program")

# show help if there are no arguments
if len(sys.argv) < 2:
    arg_parser.print_help()
    sys.exit(1)

# perform the requested action
args = arg_parser.parse_args()
if args.dump:
    dumpDatabase(args.dump, args.output, args.indented)
elif args.concat:
    concatFiles(args.concat, args.output)
