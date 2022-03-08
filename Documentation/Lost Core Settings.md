
* You can find the Lost Core settings inside Unity's Project Settings under the "Lost Core" tab.

* Tools
  * Serialization and Line Settings
  
  * Source Control Ignore File
  
  * Editor Config
  
  * Override Template Files
  
  * Rosyln Analyzers
    * Currently only StyleCop comes with the package, but you can add more
    * Add more projects like Assembly-CSharp to run it over your code.
  
  * Package Mapper
    * This is a super handy tool if you have Unity packages that you use between projects stored in GitHub.  The gist is that you clone your depot locally and tell PackageMapper where the package lives.  You can then right click the package in the Unity Editor and say map to local source, this will move it to your local github repo.  You can now make all your changes and checkin through github and push to master.  Then you right click folder and say "Lost/Package Mapper/Map Package(s) to Latest GitHub", and it will udate your manifest to point to github instead.
  
  
  * GUID Fixer

    