###############################################################################
# TWO POINTERS PATTERN REFERENCE (Unique + Correct)
###############################################################################

###############################################################################
# 1) REVERSE STRING (TWO POINTERS)
# Problem: Return the reversed version of a string s.
#
# Pattern:
# - Convert to a list (strings are immutable in Python)
# - Use left/right pointers and swap while left < right
#
# Time Complexity: O(n)
# Space Complexity: O(n)  (because we create a list of characters)
###############################################################################

def reverseString(s):
    chars = list(s)
    left = 0
    right = len(chars) - 1

    while left < right:
        # Swap characters at left and right
        chars[left], chars[right] = chars[right], chars[left]
        # Move pointers inward
        left += 1
        right -= 1

    return "".join(chars)


###############################################################################
# 2) VALID PALINDROME (SIMPLE)
# Problem: Return True if s is a palindrome (exact match), else False.
#
# Pattern:
# - Compare ends
# - Move pointers inward
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def validPalindrome(s):
    left = 0
    right = len(s) - 1

    while left < right:
        if s[left] != s[right]:
            return False
        left += 1
        right -= 1

    return True


###############################################################################
# 3) VALID PALINDROME (IGNORE NON-ALPHANUMERIC + CASE)
# Problem: Return True if s is a palindrome after:
# - ignoring non-alphanumeric characters
# - ignoring case
#
# Pattern:
# - Skip invalid characters using while loops
# - Compare lowercase characters
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def isPalindrome(s):
    left = 0
    right = len(s) - 1

    while left < right:
        # Move left pointer until it points to an alphanumeric char
        while left < right and not s[left].isalnum():
            left += 1

        # Move right pointer until it points to an alphanumeric char
        while left < right and not s[right].isalnum():
            right -= 1

        # Compare characters case-insensitively
        if s[left].lower() != s[right].lower():
            return False

        # Move inward after a successful match
        left += 1
        right -= 1

    return True


###############################################################################
# 4) TWO SUM II (SORTED ARRAY) -> RETURN 1-INDEXED POSITIONS
# Problem: Given a sorted array nums, find two numbers that add up to target.
# Return their indices as 1-indexed: [left+1, right+1]
#
# Pattern:
# - Use left/right pointers
# - If sum too small, move left up
# - If sum too large, move right down
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def twoSumPointers(nums, target):
    left = 0
    right = len(nums) - 1

    while left < right:
        current_sum = nums[left] + nums[right]

        if current_sum == target:
            return [left + 1, right + 1]  # 1-indexed
        elif current_sum < target:
            left += 1
        else:
            right -= 1


###############################################################################
# 5) FIXED SLIDING WINDOW: MAX SUM OF SUBARRAY SIZE K
# Problem: Return the maximum sum of any contiguous subarray of size k.
#
# Pattern:
# - Expand window with right pointer
# - When window size hits k, record best sum and slide by moving left
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def maxSum(nums, k):
    left = 0
    window_sum = 0
    best_sum = float("-inf")  # safer if nums can contain negatives

    for right in range(len(nums)):
        # Expand the window by adding the right element
        window_sum += nums[right]

        # When we reach window size k, update best_sum and slide window
        if right - left + 1 == k:
            best_sum = max(best_sum, window_sum)

            # Slide: remove left element and move left pointer
            window_sum -= nums[left]
            left += 1

    return best_sum


###############################################################################
# 6) VARIABLE / SHRINKING WINDOW: LONGEST SUBARRAY WITH SUM AT MOST TARGET
# Problem: Return the maximum length of a contiguous subarray whose sum <= target.
# Assumption: nums contains non-negative numbers (required for shrinking window correctness).
#
# Pattern:
# - Expand with right pointer
# - Shrink from left while sum > target
# - Track max window length
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def longestSubarrayWithSumAtMostTarget(nums, target):
    left = 0
    window_sum = 0
    max_length = 0

    for right in range(len(nums)):
        window_sum += nums[right]

        # Shrink window until it becomes valid again
        while window_sum > target:
            window_sum -= nums[left]
            left += 1

        # Update best length for a valid window
        max_length = max(max_length, right - left + 1)

    return max_length


###############################################################################
# 7) TWO POINTERS EXISTENCE CHECK (SORTED ARRAY)
# Problem: Given a sorted array nums and target, return True if any pair sums to target.
#
# Pattern:
# - Standard left/right pointer pair check
#
# Time Complexity: O(n)
# Space Complexity: O(1)
###############################################################################

def twoPointers(nums, target):
    left = 0
    right = len(nums) - 1

    while left < right:
        current_sum = nums[left] + nums[right]

        if current_sum == target:
            return True
        elif current_sum < target:
            left += 1
        else:
            right -= 1

    return False

def twoPointers(nums, target):
    left = 0
    right = len(nums) - 1

    while left < right:
        current_sum = nums[left] + nums[right]

        if current_sum == target:
            return True
        elif current_sum < target:
            left += 1
        else:
            right -= 1

    return False