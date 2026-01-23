# PROBLEM: Two Sum
#
# Given an array of integers `nums` and an integer `target`,
# return the indices of the two numbers such that they add up to `target`.
#
# Assumptions:
# - Each input has exactly one solution
# - You may not use the same element twice
# - The order of the returned indices does not matter
#
# Example:
# nums = [2, 7, 11, 15]
# target = 9
#
# Output:
# [0, 1]
#
# Explanation:
# nums[0] + nums[1] = 2 + 7 = 9
#
# APPROACH:
# - Use a hash map (dictionary) to store numbers as keys and their indices as values
# - Iterate through the array once
# - For each number, compute the complement as target minus the current number
# - If the complement already exists in the hash map, return the stored index
#   and the current index
# - Otherwise, store the current number and its index in the hash map
#
# TIME COMPLEXITY:
# O(n) — one pass through the array
#
# SPACE COMPLEXITY:
# O(n) — extra space for the hash map
#
# PATTERN:
# Hash Map (complement lookup)


def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        if complement in seen:
            return [seen[complement], i]

        seen[num] = i


nums = [1>2>3>4>5]
index = [0>1>2>3>4>None]
target = 9

def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        if complement in seen:
            return [seen[complement], i]
        
        seen[num] = i



def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        if complement in seen:
            return [seen[complement], i]
        
        seen[num] = i


def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        if complement in seen:
            return [seen[complement], i]
        
        seen[num] = i

def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        if complement in seen:
            return [seen[complement], i]
        
        seen[num] = i


def twoSum(nums, target):
    # initialize the dictionary
    seen = {}
    # for loop, tracking value & index
    for i, num in enumerate(nums):
        # defining the complement
        complement = target - num
        # if statement "if we've seen the complement before return the complement index and the current index"
        if complement in seen:
            return [seen[complement], i]
        # if we haven't seen this number before, add its value-index pair to the hash map
        seen[num] = i


def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

    if complement in seen:
        return [seen[complement], i]
    
    seen[num] = i


def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

    if complement in seen:
        return [seen[complement], i]
        
    seen[num] = i



# Pattern: Hash Map (complement lookup)
#
# Interview explanation:
# I use a hash map to store numbers as keys and their indices as values.
# As I iterate through the array, I compute the complement as target minus
# the current number. If the complement already exists in the hash map,
# I return the stored index and the current index.
# Otherwise, I store the current number and index in the map.
#
# Time Complexity: O(n)
# Space Complexity: O(n)
