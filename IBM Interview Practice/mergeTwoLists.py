# a listnode has to be defined, usually it will be given in an interview
class ListNode:
    def __init__(self, val=0, next=None):
        self.val = val
        self.next = next


# Problem:
# You are given the heads of two sorted linked lists l1 and l2.
# Merge the two lists into one sorted list and return its head.
#
# Example:
# Input: l1 = 1 -> 2 -> 4, l2 = 1 -> 3 -> 4
# Output: 1 -> 1 -> 2 -> 3 -> 4 -> 4

# Time Complexity: O(n + m)
# Each node from both linked lists is processed once while merging,
# where n and m are the lengths of the two lists.
#
# Space Complexity: O(1) extra space
# The merge is done in-place by reusing existing nodes.
# Only a few pointers are used, and no new data structures are created.


def mergeTwoLists(l1, l2):
    dummy = ListNode(0)
    tail = dummy

    while l1 and l2:
        if l1.val <= l2.val:
            tail.next = l1
            l1 = l1.next
        else:
            tail.next = l2
            l2 = l2.next

        tail = tail.next

    tail.next = l1 if l1 else l2
    return dummy.next

# Pattern: Two Pointers (Linked List Merge)
#
# Interview explanation:
# I merge the two sorted linked lists by using a dummy node to simplify edge cases.
# I maintain a tail pointer that always points to the end of the merged list.
# While both lists are non-empty, I compare their current values and attach
# the smaller node to the merged list, advancing the corresponding pointer.
# After one list is exhausted, I append the remaining nodes from the other list.
# Finally, I return the node following the dummy head.
#

