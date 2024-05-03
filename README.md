# Graph Simular CVRP Variant
## Table of Contents
1. Summary
2. Core Components
3. Color Clustering
4. Initilization
5. Threading
6. Vehicle Apportionment
7. Evaluation
8. Key Code Chunks
9. Notes

## Summary
This is a visualization tool meant to make it easier to work on and implement algorithms for node based problems such as the TSP (traveling salesman problem) and the CVRP. There are a large number of predesigned nodal problems saved that can be loaded into the scene. Then an evaluation heuristic can be selected to attempt to “solve” the nodes in the visualiser, the user can modify the parameters and location of the nodes while they are in the visualizer to change the details of the simulation. Then as the evaluation runs in processes solutions are sent to be visualized and graphed for the user to see. See diagram below.
![Diagram of Program Flow](/READMEIMGS/GS_RDME_IMG1.PNG)

## Core Components
This program has the following components with the basic functionality
* Entity Manager
  + Serves as the data container for much of the visualization and handles all global manipulation of the nodes and edges between them. 
* Converters (Multiple)
  + Multiple parsers used to interpret raw data files into useful information for the visualiser. Current supported formats are TSP (.txt) + (.xml) VRP (.vrp) and EVRP (.evrp)
* Graph Manager + Line Graph Renderer
  + Display Score Over Time
* Camera Controller + Selection Manager + Placement Manager
  + Allows the user to pan around and scroll in and out (RTS Style Movement), as well as actively manipulate the visualization, such as node color.
* Algorithms (Multiple)
  + Variety of algorithms that take in the current state of the visualizer and attempt to product solutions based on the current state of the field.
 
## Color Clustering
