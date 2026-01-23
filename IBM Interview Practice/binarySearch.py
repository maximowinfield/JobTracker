# time complexity is O(log n) time because the search space is halved on each iteration.
# space complexity is O(1) constant as we are employing a two pointer technique and not creating additional data structures


# define the binarySearch function passing, the array and target through it
def binarySearch(nums, target):
    # initialize pointers starting at the beginning and end of the array
    left = 0
    right = len(nums) - 1

    # start loop while left pointer is less than or equal to the right pointer
    while left <= right:
        # define the middle point, IMPORTANT
        mid = (left + right) // 2

        # if the middle number matches the target return the middle number
        if nums[mid] == target:
            return mid
        # otherwise if the middle number is less than the target, the left pointer moves to the right of middle by one. 
        # (halving the search)
        elif nums[mid] < target:
            left = mid + 1
        # if the middle number isn't less than the target it must be greater
        else:
            # so we will move the right pointer instead to the left of middle by one, halving the search
            right = mid - 1
    # if the target doesn't exist in the array at this point return -1
    return -1


# Pattern: Binary Search
#
# Interview explanation:
# I use two pointers to define the current search range in a sorted array.
# On each iteration, I calculate the middle index and compare its value to the target.
# If the middle value matches the target, I return its index.
# If the middle value is smaller, I move the left pointer to mid + 1.
# If the middle value is larger, I move the right pointer to mid - 1.
# This halves the search space on each iteration.
#
# Time Complexity: O(log n)
# Space Complexity: O(1)
