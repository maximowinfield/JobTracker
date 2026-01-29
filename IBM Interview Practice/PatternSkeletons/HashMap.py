###############################################################################
# HASH MAP / SET PATTERN REFERENCE
# Clean, interview-ready implementations
###############################################################################

###############################################################################
# 1) CONTAINS DUPLICATE
# Pattern: Set membership check
# Idea: If we see a value twice, return True
###############################################################################

def contains_duplicates(nums):
    seen = set()

    for num in nums:
        # If the number is already in the set, it's a duplicate
        if num in seen:
            return True
        # Otherwise, add it to the set
        seen.add(num)

    return False


###############################################################################
# 2) COUNT FREQUENCIES
# Pattern: Hash Map frequency counting
# Idea: Count how many times each number appears
###############################################################################

def count_frequencies(nums):
    seen = {}

    for num in nums:
        if num in seen:
            seen[num] += 1
        else:
            seen[num] = 1

    return seen


###############################################################################
# 3) TWO SUM
# Pattern: Hash Map complement lookup
# Idea: Store numbers we've seen and check if the complement exists
###############################################################################

def twoSum(nums, target):
    seen = {}

    for i, num in enumerate(nums):
        complement = target - num

        # If the complement exists, we found the pair
        if complement in seen:
            return [seen[complement], i]

        # Store the current number with its index
        seen[num] = i


###############################################################################
# 4) FIRST NON-REPEATING CHARACTER
# Pattern: Hash Map frequency counting + second traversal
# Idea: Count frequencies, then return the first character with count == 1
###############################################################################

def first_non_repeating(s):
    seen = {}

    # First pass: count character frequencies
    for ch in s:
        if ch in seen:
            seen[ch] += 1
        else:
            seen[ch] = 1

    # Second pass: find the first character with frequency 1
    for ch in s:
        if seen[ch] == 1:
            return ch

    return -1


###############################################################################
# 5) LONGEST SUBSTRING WITHOUT REPEATING CHARACTERS
# Pattern: Sliding Window (Two Pointers) + Hash Map
# Idea: Move the left pointer when a duplicate appears in the window
###############################################################################

def lengthOfLongestSubstring(s):
    last_seen = {}
    left = 0
    max_len = 0

    for right in range(len(s)):
        ch = s[right]

        # If the character was seen and is inside the current window,
        # move the left pointer
        if ch in last_seen and last_seen[ch] >= left:
            left = last_seen[ch] + 1

        # Update the last seen index of the character
        last_seen[ch] = right

        # Update the maximum window length
        max_len = max(max_len, right - left + 1)

    return max_len


###############################################################################
# 6) MAJORITY ELEMENT
# Pattern: Hash Map frequency counting
# Idea: Return the element that appears more than n // 2 times
###############################################################################

def majorityElement(nums):
    seen = {}
    n = len(nums)

    for num in nums:
        if num in seen:
            seen[num] += 1
        else:
            seen[num] = 1

        # Majority element appears more than half the time
        if seen[num] > n // 2:
            return num
