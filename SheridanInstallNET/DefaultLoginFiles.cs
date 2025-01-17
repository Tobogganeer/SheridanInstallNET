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

        private static readonly string BlankFile = $@"### Config
# Category is used to group files for easy enabling/disabling
Category=
# Lower orders are loaded first (services with same order loaded alphabetically)
Order=0
# Controls whether this service will be logged into by default
EnabledByDefault=true



### Commands go here...
goto slate.sheridancollege.ca
ctrl t
typeenter hi :3


{CommandHelpText}
";

        private static readonly string DefaultFileHeader = @"### === {0} - Default Login file === ###
### === {1} === ###

";

        private static readonly string FileTemplate = @"### Config
Category={0}
Order={1}
EnabledByDefault={2}

### Commands
{3}";


        static readonly string Default_Slate = @"goto slate.sheridancollege.ca
wait 3.0
tab 2
enter
wait 2.0

email
enter
wait 2.5
password
enter

# Done - wait for 2fa";
        static readonly string Default_VisualStudio_Sheridan = @"open visual studio 2022
# Click sign in and wait for it to open
tab 2
enter
wait 4.0

";
        static readonly string Default_VisualStudio_Personal = @"";
        static readonly string Default_Github = @"# Download github desktop and wait
goto https://central.github.com/deployments/desktop/desktop/latest/win32
wait 5.0

win r
typeenter downloads
wait 2.0
# Select file
up
down
enter

# Give it time to install
wait 10.0
enter
wait 3.5

# Sign in
email
tab
password
enter

# Done - wait for 2fa

";
        static readonly string Default_Unity = @"";
        static readonly string Default_Miro = @"";


        public static void CreateEmpty(string directory, string name)
        {
            LoginFile.Create(directory, name, BlankFile);
        }

        public static void CreateDefault(string directory)
        {
            throw new NotImplementedException("TODO later");
        }

        static string GetDefaultFileText(string name, string commands, string description,
            string category, int order, bool enabledByDefault)
        {
            string header = string.Format(DefaultFileHeader, name, description);
            string body = string.Format(FileTemplate, category, order, enabledByDefault, commands);
            return header + body;
        }
    }
}
