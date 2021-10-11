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
    • 3D Grid: Actors form a 3D grid. The actors can only talk to the grid neighbors.
    • Imperfect 3D Grid: Grid arrangement but one random other neighbor is selected from the list of all actors.

# Requirements
Input: The input provided (as command line to your project2) will be of the form:
    project2 <numNodes> <topology> <algorithm>

where numNodes is the number of actors involved, topology is one of full, 3D, line, imp3D, algorithm is one of gossip, push-sum.


Output: Print the amount of time it took to achieve convergence of the algorithm. Please measure the time using
... build topology
val b = System.currentTimeMillis;
..... start protocol
println(b-System.currentTimeMillis)

# What is working?

The following issues were faced during implementation :
 
1) There was a blind spot issue among the child nodes, who were not receiving the message in case of gossip algorithm (especially in line topology).
2) Obtained convergence for s,w ratio among nodes with precision of [10 power (- 10) ] ,i.e. up-to ten decimal places

After implementing a few workarounds in the code, all the topologies are converging in both Gossip and Push-sum algorithms.

1) Line, full, 3D and imp3D topologies are converging in case of Gossip algorithm  and all nodes are able to receive the rumor at-least 10 times.
2) s,w ratios are converging in all nodes for line, full, 3D and imp3D topologies in case of Push - Sum algorithm with precision of 10 decimal places.


# Largest network obtained

Gossip Algorithm: The largest network we were able to run was 10000 nodes for each topology (line, full, 3D, and imperfect 3D).

Push-Sum Algorithm: The largest network we were able to run was 500 nodes for each topology (line, full, 3D, and imperfect 3D). 