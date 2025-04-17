PG27 Simone Cormier
Cloud Computing A2

You must start by going into the raf-housing-main>raf_housing_backend folder in a CMD and run "npm run dev" to run the backend server.

After that you can start the unity project. Start from scene "Main Menu". From there you can log in with username: simone and password: simone OR username:vfs password:vfs or create a new account. This should take you to the game screen. It will load data for existing users OR create an empty game space. 

Once you press save, it either creates or overrides a "save.json" file inside the raf_housing_backend folder with the slider data, button data, username data and house position/rotaton data. There is also an "event.json" file created for starting and exiting the game. 

When you press save, it saves the JSON data both to the backend in a Save folder for each username and locally in the peristent data file that will be printed in the debug. This will also save per user.

When you log in, it will create an event file in same places as above(in backend and locally) in an Event folder with start and end data, the game score and the username saved. This will also be saved by user in both locations. 