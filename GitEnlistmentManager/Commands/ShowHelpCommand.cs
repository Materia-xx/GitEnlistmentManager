using GitEnlistmentManager.CommandSetFilters;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GitEnlistmentManager.Commands
{
    public class ShowHelpCommand : Command
    {
        public ShowHelpCommand() 
        {
            this.Documentation = "Shows help.";
        }

        private List<Type> AllCommandsTypes = new List<Type>()
        {
            typeof(ArchiveEnlistmentCommand),
            typeof(CompareSelectLeftSideCommand),
            typeof(CompareToLeftSideCommand),
            typeof(CreateBucketCommand),
            typeof(CreateDirectEnlistmentCommand),
            typeof(CreateEnlistmentAboveCommand),
            typeof(CreateEnlistmentCommand),
            typeof(CreateRepoCommand),
            typeof(DeleteBucketCommand),
            typeof(EditGemSettingsCommand),
            typeof(EditRepoSettingsCommand),
            typeof(GitCloneCommand),
            typeof(GitCreateBranchCommand),
            typeof(GitSetPullDetailsCommand),
            typeof(GitSetPushDetailsCommand),
            typeof(GitSetUserDetailsCommand),
            typeof(ListTokensCommand),
            typeof(ManageRemoteBranchesCommand),
            typeof(OpenRootSolutionCommand),
            typeof(RefreshTreeviewCommand),
            typeof(RunProgramCommand),
            typeof(ShowHelpCommand)
        };

        private List<Type> AllCommandSetFilterTypes = new List<Type>()
        {
            typeof(CommandSetFilterCloneUrlContains),
            typeof(CommandSetFilterCsmMemoryContainsKey),
            typeof(CommandSetFilterGemCompareOptionSet)
        };

        public override async Task<bool> Execute()
        {
            var helpText = new StringBuilder();
            helpText.AppendLine(@"GEM - Git Enlistment Manager

The main section of the program is a tree view that shows 4 levels of nodes. From highest to lowest these
are: Repository Collections, Repository, Buckets and Enlistments

Repository Collection: Simply a collection of repositories. These can be added through the main Gem configuration
window.

Repository: A representation of a git repository, repo for short. Note that this is just the information
about the repository, not an actual checked out clone.

Bucket: Like folders holding a collection of enlistments. The main purpose of having buckets at all is for
the default push/pull setup that the program will establish for each enlistment.

Enlistment: A cloned instances of the repo they are under. An enlistment represents a directory on your hard drive.
The branch that is set up is intended to be constant. Gem will work best if this is the case.

Push/Pull setups: The default when creating an enlistment is that pulls will be set up in a chained fashion. The
first enlistment in a bucket will pull from the remote repository, the second enlistment made in a bucket will
pull from the first enlistment and so on. The default for push on the other hand will always push the branch in
each enlistment to the remote repository (not to the parent repo).

A working example: You are a developer working on a website that sells boxes. You want a new feature in your
website to be able to sell colored boxes. You determine that before this work can be done you'll need to add
a feature that allows boxes to be shown in color on the website and a way for users to choose the color so you
create enlistment 010000.boxcolorselect to implement this. While working on this you discover that there is 
work that needs to be done in the database layer and in your project to support the notion of the selected color.
So you use the 'Add new Enlistment above' to 'inject' and enlistment above 010000.boxcolorselect and this new
enlistment is named 005000.colordbsetup. You get the db and any code needed complete in 005000.colordbsetup and
commit your changes. No PR yet because you're not sure if all of your enlistments work together correctly as 
a whole at this time. Jumping back over to 010000.boxcolorselect you do a 'git pull' which pulls in the changes
from 005000.colordbsetup and allows you to continue working on just the task meant for this enlistment. This
task is eventually done. You are able to test all enlistment features within the bucket together by running
the website in the most child bucket 010000.boxcolorselect and it looks good. Now you send out a PR for 
005000.colordbsetup which merges into master in the main repo. At the same time you also start a PR for
010000.boxcolorselect and this PR merges into the 005000.colordbsetup branch which makes it so the PR only
shows the diffs between these 2 branches instead of everything actually in that branch. The first PR is
eventually approved and merged into master. You make sure to pull in the enlistment that just had its PR
merged into master so its refs are updated. You also pull in your 010000.boxcolorselect enlistment so it
picks up the same refs update. Your work with 005000.colordbsetup is now done so you archive this enlistment
and this leaves you with just 1 enlistment in your bucket. The second PR is approved, but instead of just
merging it into the target branch you instead push it again so the remote repo branch gets the refs update
that are needed. You then change the target branch in the PR to now be master. The changes represented
by the PR will be the same as what was approved and you can now complete this PR merging it into master.

Command Sets and commands: All of the right click menus and commands you can run on the command prompt are
'command sets', including the one that is generating this text. A Command Set holds a collection of commands
and other settings like what the verb you need to type on the command line and what the right click text
displays as. A Command Set doesn't contain any logic itself to do anything. Commands on the other hand
are the building blocks that you can piece together to create a Command Set and do contain logic to do
various things. Command Sets are json files that can be edited or overridden. A default set of Command sets
are written to a folder, these can be viewed by clicking the 'open' button next to the Command Set Directories
setting in the Gem settings window.

Following is a list of all Command Sets currently loaded:
            ");

            var commandSets = Gem.Instance.CommandSets.RemoveTheOverriddenDefaultCommandSets();
            commandSets = commandSets.OrderBy(cs => cs.Placement).ThenBy(cs => cs.Verb).ToList();

            foreach (var commandSet in commandSets)
            {
                // Help only shows command sets that have a verb
                if (!string.IsNullOrWhiteSpace(commandSet.Verb))
                {
                    helpText.AppendLine($"[{commandSet.Placement}] {commandSet.Verb} - {commandSet.Documentation}");
                }
            }

            helpText.AppendLine(@"
Command Set Filters are additional bits that you can add into a command set to make it only appear
in certain situations. The following filters are available:
");
            foreach (var commandSetFilterType in this.AllCommandSetFilterTypes.OrderBy(c => c.Name))
            {
                var filter = Activator.CreateInstance(commandSetFilterType) as ICommandSetFilter;
                if (filter != null)
                {
                    helpText.AppendLine($"{commandSetFilterType.Name} - {filter.Documentation}");
                }
            }

            helpText.AppendLine(@"
Following is a list of all known Commands:
");

            foreach (var commandType in this.AllCommandsTypes.OrderBy(c => c.Name))
            {
                var cmd = Activator.CreateInstance(commandType) as Command;
                if (cmd != null)
                {
                    helpText.AppendLine($"{commandType.Name} - {cmd.Documentation}");
                }
            }

            helpText.AppendLine(@"
Managing Server Branches

In the manage server branches dialog you can recreate branches that only exist on the remote server or delete them.
When using the re-create remote branch option, keep in mind the following tips to achieve success.
* Only branches that originally existed in a bucket should be recreated in that bucket.
* Branches should be re-created in the same order they originally existed in the bucket.
");


            await Global.Instance.MainWindow.ClearCommandWindow().ConfigureAwait(false);
            await Global.Instance.MainWindow.AppendCommandLine(helpText.ToString(), Brushes.AliceBlue).ConfigureAwait(false);

            return true;
        }
    }
}
