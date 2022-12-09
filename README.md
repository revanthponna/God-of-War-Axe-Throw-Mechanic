# God-of-War-Axe-Throw-Mechanic
* This project is where I recreated the Leviathan Axe Throw mechanic from the iconic God of War (2018) game using Unity and C#.
* Click [here](https://revanthponna.github.io/God-of-War-Axe-Throw-Mechanic/) to play it on a Github page created for the project. 
* Controls: Move - Arrow Keys, Aim - Right Mouse Button, Throw Axe - Left Mouse Button, Recall Axe - Left Mouse Button.

## Project Details
* In addition to developing a third person camera controller using Unity's Cinemachine, I downloaded several animations for the character's idle, walk, aim, and throw motions from Mixamo.com. I also downloaded the model of Kratos' Leviathan Axe from Sketchfab.
* I added an animation event at the frame where I wanted the throw to occur, in order to get the throw and the animation to happen simultaneously. I then use my script's AxeThrow() function to execute the animation event. I wrote a script on the axe that modifies its Euler Angles on update in order to make it rotate.

![Axe Throw](https://user-images.githubusercontent.com/93521732/206699936-45fa9bef-3eba-458e-983c-bd38f58466fd.gif)

* I made the axe return by making it produce an arc using the Lerp function and a quadratic bezier curve. I was able to use an empty game object that was a child of my character as a centre curve point for the quadratic bezier formula by creating it.
* Final post-processing and some basic particle effects for the axe were applied for polish, such as glow for the axe and a camera shake when it is caught.

![Axe Recall](https://user-images.githubusercontent.com/93521732/206700794-3fe57712-eee9-4e16-a669-80b3b65a0b6f.gif)
