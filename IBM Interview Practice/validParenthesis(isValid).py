# valid parentheses 
# Problem:
# Given a string s containing only the characters '(', ')', '{', '}', '[' and ']',
# determine if the input string is valid.
#
# A string is considered valid if:
# 1. Every opening bracket is closed by the same type of bracket.
# 2. Brackets are closed in the correct order.
# 3. Every closing bracket has a corresponding opening bracket.
#
# Return True if the string is valid, otherwise return False.

# time complexity O(n) we only pass through the string once
# space complexity O(n) linear because a stack can grow with input size

def isValid(s):
    stack = []
    mapping = {')': '(', ']': '[', '}': '{'}

    for char in s:
        if char in mapping:
            if not stack or stack.pop() != mapping[char]:
                return False
        else:
            stack.append(char)

    return not stack



# if I go blank
def isValid(s):
    stack = []

    for char in s:
        pass

    return True

def isValid(s):
    stack = []
    mapping = {')':'(', ']':'[', '}':'{'}

    for char in s:
        if char in mapping:
            if not stack or stack.pop() != mapping[char]:
                return False
        else:
            stack.append(char)

    return not stack

# Pattern: Stack + Hash Map
#
# Interview explanation:
# I use a stack to store opening brackets and a hash map that maps
# closing brackets to their corresponding opening brackets.
# When I encounter an opening bracket, I push it onto the stack.
# When I encounter a closing bracket, I check that the stack is not empty
# and that the top of the stack matches the expected opening bracket.
# At the end, the stack must be empty for the string to be valid.
#
# Time Complexity: O(n)
# Space Complexity: O(n)
