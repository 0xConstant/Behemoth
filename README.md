# Behemoth - open source, minimal and a hack job at best
Released on a good Tuesday for educational purposes.


## Disclaimer - Must read!
This project can be used for malware research and development. The ransomware was created for educational purposes only. It has been designed to assist the general public in learning and exploring various concepts, techniques, or principles related to malware development. 

The creator of this tool and author of this documentation assume no liability for any consequences arising from the use or misuse of the tool. By using this tool, users acknowledge that they understand the limitations, assumptions, and potential risks associated with its functionality.


## Description
Behemoth's source code is pretty well explained and you won't need to know a lot to use it. The code is pretty well documented and if you hit a wall, contact me. Behemoth is not supposed to be used at a school talent show so make sure you upgrade it first.



## Update - June 30th 2023 
Behemoth's has some major shortcomings and all of those bugs/issues will be addressed in the next massive release. The next release will contain a builder along with documentation, and more advanced techniques. The release date is not yet known but it will be in the next few months.



## Behemoth's limitations
I would seriously hate to get arrested for creating Behemoth because it's not really that advanced, yet. 
I don't want to be the guy that would tell you "I told you so" but I told you so, Behemoth does have a shitload of core limitations, here are all of them:
* Requires administrator (can be removed if you don't want to wipe shadow copies)
* No code optimization; uses a high number of of threads to achieve a goal in a short span of time
* No PE injection; hides in plain sight 
* Uses symmetric encryption instead of asymmetric 
* No Anti-forensics techniques 
* No worm - doesn't spread anywhere 
* No automated privilege escalation vector 
* A sheer lack of error handling and C2 security

If you are planning to conduct the next ransomware heist, perhaps, you shouldn't do it with my ransomware. Not only does it leave a shit stain on good old Kasra but you won't get very far with the current code.


### Project structure
* app.py - this is your web application
* Behemoth/Program.cs - this is your main application and program's entry point
* Behemoth/Form1.cs - this is your GUI form
* Behemoth/App.config - these are configuration variables set through Settings.Settings


### Features:
* Disk identification
* Recursive file search
* Random string generation
* UID generator
* AES encryption/descryption
* File rename
* Shadow copies deletion
* Killing all open processes
* Self-destruct 
* Shortcut creation
* Automatically delete users after 8 hours from database


### Usage
Clone this repository, move app.py elsewhere and create a Python virtualenv and activate it:
```
python3 -m venv venv
source venv/bin/activate
```

Install all of it's dependencies and then launch flask shell to create tables:
```
$ flask shell
>>> db.create_all()
>>> exit()
```
Run the flask app like this:
`flask run --host 0.0.0.0 --cert adhoc`
Basically, we are creating a HTTPs server without doing jack, this was used as a testbed to hide POST data because Hidden Tear didn't do a very bright job at that.

Open Behemoth in visual studio and edit the source code at Behemoth/Program.cs, replace the IP `https://10.0.0.115:5000` with your server's IP and port. Build the solution and execute it.


### Live test
Here is what happens when Behemoth is launched:
1. The first step of Behemoth is closing all opened proceeses
2. Once launched, Behemoth makes a executable shortcut in Desktop to make it easier to launch if closed
3. You can also see that machine usage is going high 
4. A few seconds later, you notice that files within your Desktop get a different file extension and they are encrypted
5. A file named file_paths.txt is written to Desktop and it contains paths to all encrypted files 
6. A few seconds later you notice a GUI being opened 
7. In your server logs, you will notice an IP address making connection to your C2 on /stage1 route
8. The program contains a UID created by combining your hostname, username and Mac address
9. A unique fake crypto address is also generated (This can be replaced with coinbase API)
10. The program has calculated the time from when it was started to 8 hours forward and notices you how long you have to pay the ransom
11. Clicking "Check Payment" shows that payment has failed but it also sends another request to /victims route
12. The victims table has a creation & expiration date which automatically removes users after 8 hours 
13. Changing value of payment column from 0 to 1 means that you have paid your ransom - use SQL database browser to open user_data.db
14. When you click "Check Payment" again, you notice a message that says "Payment successful..."
15. You will notice a progress bar showing the decryption process for files 
16. Application exits and file self-destructs 
17. The desktop shortcut of Behemoth no longer works


### Code 
The program by default runs as administrator, this can be changed but you will have to remove `SCWipe()` and all of it's calls. Here is how:
1. Remove `SCWipe();` from line 455 on Behemoth/Program.cs
2. Find Behemoth/app.manifest and replace the following line:

`<requestedExecutionLevel  level="requireAdministrator" uiAccess="false" />`

With this:

`<requestedExecutionLevel level="asInvoker" uiAccess="false" />`

That should do it.


## Contact me
Somehow, deep down in my heart, I know that some of you are going to face technical difficulties going through this code. It can be challenging to get through all this on your own but a friend can help, that friend could be me. 

Get in touch with me on Discord: @c0nstant

Send me an email: kasra.constant@proton.me
