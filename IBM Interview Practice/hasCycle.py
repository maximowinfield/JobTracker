# Problem:
# Given the head of a linked list, determine if the list
# contains a cycle.
#
# A cycle exists if a node's next pointer points to
# a previous node in the list.

# Time Complexity: O(n)
# Space Complexity: O(1)

def hasCycle(head):
    slow = head
    fast = head

    while fast and fast.next:
        slow = slow.next
        fast = fast.next.next

        if slow == fast:
            return True

    return False

# Pattern: Fast and Slow Pointers (Linked List)
#
# Interview explanation:
# I use two pointers that move at different speeds through the linked list.
# The slow pointer moves one node at a time, while the fast pointer moves
# two nodes at a time. If the list contains a cycle, the fast pointer will
# eventually catch up to the slow pointer and they will meet.
# If the fast pointer reaches the end of the list, then no cycle exists.
#
# Time Complexity: O(n)
# Space Complexity: O(1)
