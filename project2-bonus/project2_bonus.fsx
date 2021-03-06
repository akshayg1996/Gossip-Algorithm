// ActorSayHello.fsx
#time "on"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
// #load "Bootstrap.fsx"

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let mutable ExecutionShutDownFlag = false
let globalMap =new Dictionary<int, List<int>>()
let globalIndMap =new Dictionary<int, int>()

let args : string array = fsi.CommandLineArgs |>  Array.tail//
//fetch the first argument - total number of nodes in the system
let totalNumberOfNodes = args.[0] |> int
//fetch number of nodes that should fail
let numberOfFailedNodes = args.[1] |> int
//fetch the second argument - topology name
let topologyName = args.[2]
//fetch the third argument - Algorithm
let algorithmName = args.[3]


//types for match cases
type InputTupleForSuperVisor = int * string 
type FloodingFlagSupervisor = string
type PushSumFlagSupervisor = string * int * double
type FloodingFlagChildNode = string 
type FloodingFlagChildNode2 = string * int
type PushSumflagChildNode =  double * double
type PushSumflagChildNode2 =  string * double * double

//counter to keep track of converged nodes
let mutable countOfNodes : int = 0
let system = ActorSystem.Create("ActorSystemSupervisor")
let childActorNodesRef : Object array = Array.zeroCreate totalNumberOfNodes  

//keeps the randomly selcted node Indexes for deliberate failures
let nodeFailureList = new List<int>()


//this method builds Full topology
let fullTopology (numberOfNodesInCluster : int) =
    for i in 0 .. numberOfNodesInCluster - 1 do
       globalIndMap.Add(i,0)
       let referenceList = new List<int>()
       for j in 0 .. numberOfNodesInCluster - 1 do
           if(i <> j) then
               referenceList.Add(j)
       globalMap.Add(i,referenceList)
  



//this method builds Line topology for given number of nodes
let lineTopology (numberOfNodesInCluster : int) =
    printfn ("inside lineTopology")
    for i in 0 .. numberOfNodesInCluster - 1 do
        globalIndMap.Add(i,0)
        if(i = 0) then
            let referenceList1 = new List<int>()
            referenceList1.Add(i+1)
            globalMap.Add(i,referenceList1) 
        else if (i = numberOfNodesInCluster - 1) then
                 let referenceList2 = new List<int>()
                 referenceList2.Add(i-1)
                 globalMap.Add(i,referenceList2) 
             else          
                let referenceList3 = new List<int>()
                referenceList3.Add(i-1)
                referenceList3.Add(i+1)
                globalMap.Add(i,referenceList3)
    

//this method builds the 3D topology
let topology3DMap (numberOfNodesInCluster : int) =
        let cubeRootValue: int =Convert.ToInt32(Math.Ceiling(Math.Cbrt(float numberOfNodesInCluster)))
        let mutable start:int =0
        for j in 1 .. cubeRootValue+1 do

            let mutable stop:int=j*cubeRootValue*cubeRootValue
            for i in start .. stop-1 do
                if i<numberOfNodesInCluster && i>=0 then
                    globalIndMap.Add(i,0)
                    let neighbourList = new List<int>()
                    if((i-cubeRootValue) < start) then
                        printf(" ")
                    else
                        neighbourList.Add(i-cubeRootValue)

                    if(not ((i+cubeRootValue) < (numberOfNodesInCluster) && (i+cubeRootValue) < stop)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+cubeRootValue)

                    if(i % cubeRootValue = 0) then
                        printf(" ")
                    else
                        neighbourList.Add(i-1)

                    if(((i+1) % cubeRootValue = 0 || (i+1) >= numberOfNodesInCluster)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+1)
                    
                    if (not((i-(cubeRootValue*cubeRootValue))>0)) then
                        printf(" ")
                    else
                        neighbourList.Add(i-(cubeRootValue*cubeRootValue))

                    if (not((i+(cubeRootValue*cubeRootValue))<numberOfNodesInCluster)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+(cubeRootValue*cubeRootValue))
                    globalMap.Add(i,neighbourList)
            start<- stop


//this method builds the imperfect 3D map topology
let imperfect3dMapTopology (numberOfNodesInCluster : int) =
        let cubeRootValue: int =Convert.ToInt32(Math.Ceiling(Math.Cbrt(float numberOfNodesInCluster)))
        let mutable start:int =0
        for j in 1 .. cubeRootValue+1 do

            let mutable stop:int=j*cubeRootValue*cubeRootValue
            for i in start .. stop-1 do
                if i<numberOfNodesInCluster && i>=0 then
                    globalIndMap.Add(i,0)
                    let neighbourList = new List<int>()
                    if((i-cubeRootValue) < start) then
                        printf(" ")
                    else
                        neighbourList.Add(i-cubeRootValue)

                    if(not ((i+cubeRootValue) < (numberOfNodesInCluster) && (i+cubeRootValue) < stop)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+cubeRootValue)

                    if(i % cubeRootValue = 0) then
                        printf(" ")
                    else
                        neighbourList.Add(i-1)

                    if(((i+1) % cubeRootValue = 0 || (i+1) >= numberOfNodesInCluster)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+1)
                    
                    if (not((i-(cubeRootValue*cubeRootValue))>0)) then
                        printf(" ")
                    else
                        neighbourList.Add(i-(cubeRootValue*cubeRootValue))

                    if (not((i+(cubeRootValue*cubeRootValue))<numberOfNodesInCluster)) then
                        printf(" ")
                    else
                        neighbourList.Add(i+(cubeRootValue*cubeRootValue))
                    let mutable randomInt = System.Random().Next(numberOfNodesInCluster)
                    while neighbourList.Contains(randomInt) do
                        randomInt <- Random().Next(numberOfNodesInCluster)
                    neighbourList.Add(randomInt)
                    globalMap.Add(i,neighbourList)
            start<- stop


// this code Block defines a child actor node 
type ChildActorNode(nodeIndex : int , lengthOfNeighboursList : int , supervisorActorNodeRef : Object ) =
  inherit Actor()
  let supervisorNodeObj = unbox supervisorActorNodeRef
  let mutable floodingMessageCounter : int = 0
  let fixHelloCounts : int = 10
  let mutable sNum : double = double (double nodeIndex / double 1000000000 )
  let mutable wDenom : double = double 1
  let pushSumCount : int = 0
  let pushSumArray : double array = Array.create 3 (double 100000)
  let mutable swRatioPrevious1 : double = double 100000
  let mutable pushSumConvergenceFlag : int = 0
  override x.OnReceive message =
     match message with
     | :? FloodingFlagChildNode as msg -> if floodingMessageCounter < fixHelloCounts then
                                                floodingMessageCounter <- floodingMessageCounter + 1

                                                //if node has received message twice and is eligible for failure, kill the node
                                                if ( floodingMessageCounter = 2 && nodeFailureList.Contains(nodeIndex) ) then
                                                        let boolval = nodeFailureList.Remove(nodeIndex)
                                                        printfn "Node : %i is failed " nodeIndex  
                                                        x.Self.Tell(PoisonPill.Instance)
                                                if (floodingMessageCounter = fixHelloCounts) then
                                                         system.ActorSelection("user/SupervisorActorNode") <! "done"
                                                let randomInt = System.Random().Next(lengthOfNeighboursList)
                                                let neighbourNode : int = (globalMap.[nodeIndex]).Item(randomInt)
                                                let nodePathNbr : string = "user/ChildActorNode_" + string neighbourNode
                                                let nodePathOwn : string = "user/ChildActorNode_" + string nodeIndex
                                                system.ActorSelection(nodePathNbr) <! "Hello"
                                                system.ActorSelection(nodePathOwn) <! ("Hello",1)
                                           else
                                                let nodePathOwn : string = "user/ChildActorNode_" + string nodeIndex
                                                system.ActorSelection(nodePathOwn) <! ("Hello",1)

                                      
               
     | :? FloodingFlagChildNode2 as msg -> let randomInt = System.Random().Next(lengthOfNeighboursList)
                                           let neighbourNode : int = (globalMap.[nodeIndex]).Item(randomInt)
                                           let nodePathNbr : string = "user/ChildActorNode_" + string neighbourNode
                                           system.ActorSelection(nodePathNbr) <! "Hello"

     | :? PushSumflagChildNode as msg -> let (sVal,wVal) = unbox<PushSumflagChildNode> msg
                                         if pushSumConvergenceFlag < 3 then
                                                sNum <- sNum + sVal
                                                wDenom <- wDenom + wVal
                                                let currentRatio : double = sNum/wDenom                    
                                                sNum <- sNum / 2.0
                                                wDenom <- wDenom / 2.0
                                                let diff : double =  swRatioPrevious1 - currentRatio
                                                floodingMessageCounter <- floodingMessageCounter + 1

                                                //if node has received message twice and is eligible for failure, kill the node
                                                if ( floodingMessageCounter = 2 && nodeFailureList.Contains(nodeIndex) ) then
                                                        let boolval = nodeFailureList.Remove(nodeIndex)
                                                        printfn "Node : %i is failed " nodeIndex  
                                                        x.Self.Tell(PoisonPill.Instance)

                                                if Math.Abs(swRatioPrevious1 - currentRatio) <= 0.0000000001 then
                                                    pushSumConvergenceFlag <- pushSumConvergenceFlag + 1
                                                    if (pushSumConvergenceFlag = 3) then
                                                        system.ActorSelection("user/SupervisorActorNode") <! ("PushDone", nodeIndex , currentRatio)
                                                else
                                                    pushSumConvergenceFlag <- 0
                                                swRatioPrevious1 <- currentRatio
                                                let randomInt1 = System.Random().Next(lengthOfNeighboursList)
                                                let neighbourNode1 : int = (globalMap.[nodeIndex]).Item(randomInt1)
                                                let nodePathNbr1 : string = "user/ChildActorNode_" + string neighbourNode1
                                                let nodePathOwn1 : string = "user/ChildActorNode_" + string nodeIndex
                                                system.ActorSelection(nodePathNbr1) <! (sNum, wDenom)
                                                system.ActorSelection(nodePathOwn1) <! ("pass", sNum, wDenom)
                                         else
                                                let nodePathOwn1 : string = "user/ChildActorNode_" + string nodeIndex
                                                system.ActorSelection(nodePathOwn1) <! ("pass",sVal/2.0, wVal/2.0)


     | :? PushSumflagChildNode2 as msg -> let (messageVal1,sVal1,wVal1) = unbox<PushSumflagChildNode2> msg
                                          let randomInt = System.Random().Next(lengthOfNeighboursList)
                                          let neighbourNode : int = (globalMap.[nodeIndex]).Item(randomInt)
                                          let nodePathNbr : string = "user/ChildActorNode_" + string neighbourNode
                                          system.ActorSelection(nodePathNbr) <! (sVal1,wVal1)

     | _ -> failwith "wrong message format for child node"
                                          
                                          


//this code block defines a supervisor actor node which supervises all processes
type SupervisorActorNode(numberOfNodesInCluster : int , numberOfFailedNodes : int , topologyName : string, algorithmName: string ) =
  inherit Actor()
  let trackRatio : double array = Array.zeroCreate numberOfNodesInCluster
  override x.OnReceive message =
    match message with
    | :? InputTupleForSuperVisor as msg ->  //let childActorNodesRef : Object array = Array.zeroCreate numberOfNodesInCluster       //
                                            printfn "inside supervisor args : %i %s %s" numberOfNodesInCluster topologyName algorithmName 
                                            match topologyName with                                       
                                                | "line" -> lineTopology(numberOfNodesInCluster)

                                                | "full" -> fullTopology(numberOfNodesInCluster)

                                                | "3D"  ->  topology3DMap(numberOfNodesInCluster)

                                                | "imp3D"  -> imperfect3dMapTopology(numberOfNodesInCluster)

                                                | _ -> failwith "wrong input for topologyname"
                                            

                                            for i in 0 .. numberOfNodesInCluster - 1  do                                            
                                                 if globalMap.ContainsKey(i) then
                                                       let lengthOfNeighboursList : int  = (globalMap.Item(i)).Count
                                                       let nodeName : string = "ChildActorNode_"+ (string i)                                              
                                                       childActorNodesRef.[i] <- box (system.ActorOf(Props(typedefof<ChildActorNode>,[|box i;  box lengthOfNeighboursList; box x.Self |]), nodeName))
                                                 else
                                                        printfn(" ")
                                                
                                            //randomly select nodes ans save their ids in failure list for deliberate failure 
                                            for i in 1 .. numberOfFailedNodes do
                                                let randomInt = System.Random().Next(numberOfNodesInCluster)
                                                nodeFailureList.Add(randomInt)

                                            if (algorithmName = "gossip") then
                                                 printfn ("saying hello to one child node")
                                                 let randomInt = System.Random().Next(numberOfNodesInCluster)
                                                 let varObj  = unbox childActorNodesRef.[randomInt]
                                                 varObj <! ("Hello")
                                            else
                                                 if algorithmName = "push-sum" then
                                                     let randomInt = System.Random().Next(numberOfNodesInCluster)
                                                     let varObj  = unbox childActorNodesRef.[randomInt]
                                                     varObj <! (double 0.0,double 1.0)


    | :? FloodingFlagSupervisor as msg ->  countOfNodes <- countOfNodes + 1
                                           printfn "count of converged nodes: %i "  countOfNodes 
                                           if (countOfNodes = numberOfNodesInCluster) then
                                                   printfn ("Flooding Done , Gossip completed : All nodes in the cluster received rumor 10 times")
                                                   ExecutionShutDownFlag <- true
                                                   printfn "cluster shutting down"

    | :? PushSumFlagSupervisor as msg -> countOfNodes <- countOfNodes + 1
                                         printfn "count of converged nodes : %i "  countOfNodes 
                                         let (pushSumDone,nodeId,swRatio) = unbox<PushSumFlagSupervisor> msg
                                         trackRatio.[nodeId] <- swRatio
                                         if countOfNodes = numberOfNodesInCluster then
                                             printfn("Push-Sum execution completed : S/w ratio convereged in all nodes")
                                             for i in 0 .. numberOfNodesInCluster - 1  do
                                                  printfn "Node = %i  :  S/W Ratio is = %s " (i + 1) (string trackRatio.[i])
                                             ExecutionShutDownFlag <- true
                                             printfn "cluster shutting down"
    
    | _ -> failwith "wrong message format for supervisor"




printfn "Total Number of Nodes in the cluster : %A \n" totalNumberOfNodes
printfn "Topology  : %A \n" topologyName
printfn "Algorithm  : %A \n" algorithmName

//creating supervisor actor object
let GossipSupervisor = system.ActorOf(Props(typedefof<SupervisorActorNode>,[|box totalNumberOfNodes; box numberOfFailedNodes ; box topologyName; box algorithmName |]),"SupervisorActorNode")
printfn ("supervisor created")


let startTimeVal = DateTime.Now


let timer = Diagnostics.Stopwatch.StartNew()
GossipSupervisor <! (totalNumberOfNodes,topologyName)

//the loop terminates when either topology fully converges or timeout happens
while not ExecutionShutDownFlag && ((DateTime.Now - startTimeVal).TotalMinutes < 5.0 ) do
        ignore ()

timer.Stop()
printfn "Execution completed or Timed out ,Total number of converged nodes : %i" countOfNodes
