# define a node in a linked list
class Node:
    # define constructor to initialize the node
    def __init__(self, data):
        #assign data for this node
        self.data = data
        # reference pointer to the next node in the list
        self.next = None  # also make None as the default value for next.

# define the count_nodes function, passing the first node (head) through it
def count_nodes(head):
    # assuming that head != None
    #count starts at 1 because the head counts as 1
    count = 1
    # current default is at the head (We are starting the count at the head)
    current = head
    # start the loop (iteration, guard condition while we have a node)
    while current.next is not None:
        #go to the next node and increment a count
        current = current.next
        count += 1
    # eventually we hit null (the end of the list) and return the count we have
    return count

# create individual nodes, defining each node name, and the data it holds.
nodeA = Node(6)
nodeB = Node(3)
nodeC = Node(4)
nodeD = Node(2)
nodeE = Node(1)

# define the relationship (pointers) between the nodes
nodeA.next = nodeB
nodeB.next = nodeC
nodeC.next = nodeD
nodeD.next = nodeE

print("This linked list's length is: (should print 5)")
print(count_nodes(nodeA))
