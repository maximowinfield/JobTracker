def isPalindrome(s: str) -> bool:
    left = 0
    right = len(s) - 1

    while left < right:
        if not s[left].isalnum():
            left += 1
            continue

        if not s[right].isalnum():
            right -= 1
            continue

        if s[left].lower() != s[right].lower():
            return False
        
        left += 1
        right -= 1

    return True

def isPalindrome(s: str) -> bool:
    left = 0
    right = len(s) - 1

    while left < right:
        if not s[left].isalnum():
            left += 1
            continue

        if not s[right].isalnum():
            right -= 1
            continue

        if s[left].lower() != s[right].lower():
            return False
        
        left += 1
        right -= 1

    return True 

def isPalindrome(s: str) -> bool:
    left = 0
    right = len(s) - 1

    while left < right:
        if not s[left].isalnum():
            left += 1
            continue

        if not s[right].isalnum():
            right -= 1
            continue

        if s[left].lower() != s[right].lower():
            return False
        
        left += 1
        right -= 1

    return True

# Time complexity is O(n) as we pass through the array once
# Space complexity is O(1) constant as no new data structures are used.

# Pattern: Two Pointers
#
# Interview explanation:
# I use two pointers starting at the beginning and end of the string.
# I skip any non-alphanumeric characters using isalnum(), normalize characters
# using lower(), and compare them while moving the pointers inward.
# If a mismatch occurs at any point, I return False.
# If all valid characters match, I return True.
#
# Time Complexity: O(n)
# The left and right pointers move inward across the string,
# checking each character at most once.
#
# Space Complexity: O(1)
# Only pointer variables are used, and no additional data structures
# are created that grow with input size.

# Another is_palindrome solution using a stack

# we define the function, and pass the string through it
def is_palindrome(s):
    # initialize a stack, a stack is like a stack of plates, Last in, first out.
    stack = []

    # Push all characters onto the stack
    for char in s:
        stack.append(char)

    # Compare characters with popped values
    for char in s:
        # if the current character doesn't equal the current stack pop, then it's not
        if char != stack.pop():
            # a palindrome
            return False

    return True
# Time Complexity: O(n)
# - The string is traversed twice:
#   1) Once to push all characters onto the stack
#   2) Once to pop and compare each character
# - O(n) + O(n) simplifies to O(n)

# Space Complexity: O(n)
# - The stack stores all n characters from the string
