using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheridanInstallNET
{
    public class DefaultLoginFiles
    {
        private static readonly string CommandHelpText = @"### Commands
# type [text] - types the given text
# enter - hits enter
# tab - hits tab (useful for navigating between controls)
# tab [amount] - hits tab a bunch of times
# tabenter - hits tab and then enter
# typeenter [text] - types and then hits enter

# email - types the email for this account
# password - types the password for this account

# open [program] - opens the given program
# wait [seconds] - delays operation for the given amount of time
# goto [url] - opens Google and goes to that URL

# win [key] - presses window key + key
# ctrl [key] - presses ctrl + key
# shift [key] - presses shift + key
# up - presses up arrow key
# down - presses down arrow key";

        private static readonly string BlankFile = $@"
### Config
# Category is used to group files for easy enabling/disabling
Category=
# Lower orders are loaded first (services with same order loaded alphabetically)
Order=0
# Controls whether this service will be logged into by default
EnabledByDefault=true



# Commands go here...
goto slate.sheridancollege.ca
ctrl t
typeenter hi :3


{CommandHelpText}
";

        public static void CreateEmpty(string directory, string name)
        {
            LoginFile.Create(directory, name, BlankFile);
        }

        internal static void CreateDefault(string directory)
        {
            throw new NotImplementedException("TODO later");
        }
    }
}
