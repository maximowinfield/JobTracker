# So I have to return True if any value appears at least twice in an array nums, returning false if every element is distinct
# I would use a set to track values I've already seen
# time complexity: I would only need to loop through the nums array once so O(n)
# space complexity: O(n) linear, because the set can grow with thge number of elements.

def containsDuplicate(nums):
    seen = set()

    for num in nums:
        if num in seen:
            return True
        seen.add(num)

    return False

# define the function, we pass the nums array through it
def containsDuplicate(nums):
    # initialize a set
    seen = set()
    # start the loop for every number in nums array
    for num in nums:
        # if the number is in the seen set
        if num in seen:
            # return true we have seen this before
            return True
        # otherwise add it to the seen set
        seen.add(num)
    # return false if we haven't seen anything at all in the array
    return False

# Pattern: Set (membership check)
#
# Interview explanation:
# I use a set to track values I have already seen while iterating through the array.
# For each element, if it already exists in the set, I immediately return True,
# since that means a duplicate was found. Otherwise, I add the element to the set.
# If the loop finishes without finding duplicates, I return False.
#
# Time Complexity: O(n)
# Space Complexity: O(n)
