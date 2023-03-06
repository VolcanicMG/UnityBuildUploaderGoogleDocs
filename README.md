# UnityBuildUploaderGoogleDocs
This simple program uploads your Unity build file (On build) to a specific google docs location. *(Manual configuration required)*

# Setup
Step 1: Create an account on `https://console.cloud.google.com/`, and create a new project. Next head over to the API libarary and install the google drive API.

Step 2: Create a Google Drive API key so we can use google services to upload the build file. Generate the .json file and put it in a goods spot since it containes sensitive information that allows the service to connect to upload the file. Next point `PathToServiceAccountKeyFile` to the .json file.

Step 2: Compile the code with your own private google data (You will need the `Google Apis NuGet packages for google drive`) then move the exe to the same directory as the unity build gets put into. 

Step 3: Add the `BuildCounter` script to the unity project to be able to upload the build file whenever we create a build. (Double check that the `UploadFileName` has the same name as your build file name) 

Step 4: Add the `BuildScriptableObject` class to the unity project.

Step 5: Run a build and wait for the program to start.

### The program by default will run in the background when you start a upload.
