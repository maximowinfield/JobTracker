###############################################################################
# SIMPLE TRAVERSAL PATTERN REFERENCE (Unique + Correct)
#
# Mental Model:
# answer = first_element (or a starting value)
# for each element:
#     update answer if needed
# return answer
###############################################################################


###############################################################################
# 1) IS SORTED
# Problem: Return True if nums is sorted in non-decreasing order, else False.
# Pattern: Simple traversal + track previous element
###############################################################################

def is_sorted(nums):
    # An empty list (or single element) is already sorted
    if not nums:
        return True

    # Store the previous value to compare against
    prev = nums[0]

    # Start from the second element and compare each to the previous
    for num in nums[1:]:
        # If current element is smaller than previous, array is not sorted
        if num < prev:
            return False
        # Update prev to the current number for the next comparison
        prev = num

    # If we never found an out-of-order pair, it is sorted
    return True


def isSorted(nums):
    if not nums:
        return True
    
    prev = nums[0]

    for num in nums:
        if num < prev:
            return False
        prev = num
    return True


###############################################################################
# 2) LINEAR SEARCH (DOES EXIST)
# Problem: Return the index of target in nums, or -1 if not found.
# Pattern: Simple traversal + equality check
###############################################################################

def does_exist(nums, target):
    # If the array is empty, target cannot exist
    if not nums:
        return -1

    # Scan each index until we find the target
    for i in range(len(nums)):
        if nums[i] == target:
            return i

    # Target not found
    return -1


###############################################################################
# 3) MAXIMUM VALUE
# Problem: Return the maximum value in nums, or None if nums is empty.
# Pattern: Track best-so-far
###############################################################################

def maximum(nums):
    # No maximum exists in an empty list
    if not nums:
        return None

    # Start with the first element as the best maximum so far
    max_value = nums[0]

    # Update max_value whenever we find a larger number
    for num in nums:
        if num > max_value:
            max_value = num

    return max_value


###############################################################################
# 4) MINIMUM VALUE
# Problem: Return the minimum value in nums, or None if nums is empty.
# Pattern: Track best-so-far
###############################################################################

def minimum(nums):
    # No minimum exists in an empty list
    if not nums:
        return None

    # Start with the first element as the best minimum so far
    min_value = nums[0]

    # Update min_value whenever we find a smaller number
    for num in nums:
        if num < min_value:
            min_value = num

    return min_value


###############################################################################
# 5) SECOND LARGEST (DISTINCT)
# Problem: Return the second largest DISTINCT value in nums, or None if it doesn't exist.
# Pattern: Track first-largest and second-largest
###############################################################################

def second_largest(nums):
    # Need at least two values to have a second largest
    if len(nums) < 2:
        return None

    first = float("-inf")
    second = float("-inf")

    for num in nums:
        # If we found a new largest, demote first to second
        if num > first:
            second = first
            first = num

        # Update second if num is between first and second AND is distinct
        elif first > num > second:
            second = num

    # If second never updated, no distinct second largest exists
    return None if second == float("-inf") else second


###############################################################################
# 6) TOTAL SUM
# Problem: Return the sum of all numbers in nums (0 if empty).
# Pattern: Running total
###############################################################################

def total(nums):
    total_sum = 0

    for num in nums:
        total_sum += num

    return total_sum


###############################################################################
# 7) COUNT POSITIVE NUMBERS
# Problem: Return how many numbers in nums are greater than 0.
# Pattern: Counter
###############################################################################

def count_greater_than_zero(nums):
    # For empty input, there are 0 positives
    if not nums:
        return 0

    count = 0

    for num in nums:
        if num > 0:
            count += 1

    return count


###############################################################################
# 8) COUNT EVEN NUMBERS
# Problem: Return how many numbers in nums are even.
# Pattern: Counter + modulo check
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def totalEven(nums):
    evens = 0

    for num in nums:
        # Even numbers have remainder 0 when divided by 2
        if num % 2 == 0:
            evens += 1

    return evens


