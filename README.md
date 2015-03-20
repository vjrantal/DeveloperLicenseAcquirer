DeveloperLicenseAcquirer
------------------------

Introduction
------------

A project based on an answer at http://stackoverflow.com/a/13348267 which uses the Microsoft UI Automation framework to go through the license acquisition process.

A developer license is required to run Windows Store apps (https://msdn.microsoft.com/en-us/library/windows/apps/hh974578.aspx), but in environments like continuous integrations servers, it is not always possible to do manual user interactions.

Usage
-----

The executable needs to be started by passing three command-line arguments:

```
DeveloperLicenseAcquirer.exe "<path-to-tailored-deploy-executable" "<username>" "<password>"
```

An example of the first argument is "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\TailoredDeploy.exe".
