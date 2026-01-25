# Problem:
# Given the root of a binary tree, return its maximum depth.
#
# The maximum depth is the number of nodes along the longest
# path from the root node down to the farthest leaf node.


def maxDepth(root):
    if root is None:
        return 0

    left_depth = maxDepth(root.left)
    right_depth = maxDepth(root.right)

    return 1 + max(left_depth, right_depth)

# Pattern: Tree Traversal (Recursion)
#
# Interview explanation:
# I solve this problem using recursion by computing the depth of the left and
# right subtrees. If the current node is None, I return 0 as the base case.
# Otherwise, I recursively calculate the maximum depth of both subtrees and
# return one plus the larger of the two values.
# This ensures I count the longest path from the root to a leaf node.
#
# Time Complexity: O(n)
# We visit each node in the tree exactly once to compute the depth,
# so the runtime grows linearly with the number of nodes.
#
# Space Complexity: O(h)
# The extra space comes from the recursion call stack.
# At most, there is one recursive call per level of the tree,
# where h is the height of the tree.


# defining the function
def maxDepth(root):
    # to avoid a null root
    if root is None:
        return 0
    
    # setting the left and right depth, or "finding" using the maxDepth() method
    left_depth = maxDepth(root.left)
    right_depth = maxDepth(root.right)

    # returning 1+ because the root depth is always 1 + the maximum of the two left and right depths. 
    # Giving a total maxDepth
    return 1 + max(left_depth, right_depth)


