Hi,

I made this application as part of my bachelor's thesis. It is intended to be a teaching- and to some extent a learning-tool for modern computer graphics. Without any prior knowledge, matrix transformations and lighting algorithms can be hard to grasp and this is my attempt at helping newcomers understand everything a bit quicker. 

I am releasing this as open-source, so feel free to make modifications if you want to! 

Adding more models is easily done, just import your model files into Unity, create a new "Model Preset" (right click the project explorer, select "Create", it should be the fourth entry) and add it to the ModelPicker-prefab. If you don't provide a flat-shaded version, one will be generated. This might take a while so I'd suggest importing an actual flat shaded version as well.

If you want more lighting setups for the lighting screen, those are scriptable objects too. Just have a look at the ScriptableObejcts-Folder to see how they are set up. You'll also have to add it to the List in the LightingScreen-Prefab (under Prefabs/canvases), otherwise it won't show up in the selections.

