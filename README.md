# Gossip Protocol Algorithm and Push-Sum algorithm
Implementing Gossip Algorithm for information propagation and Push-Sum algorithm for sum computation

# Team Members
1. Akshay Ganapathy (UFID - 3684-6922)
2. Kamal Sai Raj Kuncha (UFID - 4854-8114)

# Problem definition
As described in class Gossip type algorithms can be used both for group communication and for aggregate computation. The goal of this project is to determine the convergence of such algorithms through a simulator based on actors written in F#. Since actors in F# are fully asynchronous, the particular type of Gossip implemented is the so called Asynchronous Gossip.

# Topologies
The actual network topology plays a critical role in the dissemination speed of Gossip protocols. As part of this project you have to experiment with various topologies. The topology determines who is considered a neighboor in the above algorithms.
    • Full Network : Every actor is a neighbor of all other actors. That is, every actor can talk directly to any other actor.
    • Line : Actors are arranged in a line. Each actor has only 2 neighbors (one left and one right, unless you are the first or last actor).
    • 2D Grid: Actors form a 2D grid. The actors can only talk to the grid neighbors.
    • Imperfect 2D Grid: Grid arrangement but one random other neighbor is selected from the list of all actors.
    • 3D Grid: Actors form a 3D grid. The actors can only talk to the grid neighbors.
    • Imperfect 3D Grid: Grid arrangement but one random other neighbor is selected from the list of all actors.

# Requirements
Input: The input provided (as command line to your project2) will be of the form:
    project2 <numNodes> <topology> <algorithm>

where numNodes is the number of actors involved (for 2D based topologies you can round up until you get a square), topology is one of full, 2D, line,
imp2D, algorithm is one of gossip, push-sum.


Output: Print the amount of time it took to achieve convergence of the algorithm. Please measure the time using
... build topology
val b = System.currentTimeMillis;
..... start protocol
println(b-System.currentTimeMillis)

# Actor modeling
In this project you have to use exclusively the actor facility in F# (projects that do not use multiple actors or use any other form of parallelism will receive no credit).

# README file
In the README file, you have to include the following material:
    • Team members
    • What is working
    • What is the largest network you managed to deal with for each type of topology and algorithm

# Report.pdf
For each type of topology and algorithm, draw the dependency of convergence time as a function of the size of the network. You can overlap different topologies on the same graph, i.e. you can draw 4 curves, one for each topology and produce only 2 graphs for the two algorithms. Write about any interesting finding of your experiments in the report as well and mention the team members. You can produce Report.pdf in any way you like, for example using spreadsheet software. You might have to use logarithmic scales to have a meaningful plot.