###############################################################################
# STACKS PATTERN REFERENCE (Unique + Correct)
###############################################################################

###############################################################################
# 1) VALID PARENTHESES (BRACKETS)
# Problem:
# Given a string s containing '(){}[]', return True if it is valid.
#
# A string is valid if:
# - Every closing bracket has a matching opening bracket
# - Brackets close in the correct order
#
# Pattern:
# - Stack to store opening brackets
# - Hash map to match closing -> opening
#
# Time Complexity: O(n)
# Space Complexity: O(n)
###############################################################################

def isValid(s):
    stack = []
    # Map each closing bracket to its matching opening bracket
    mapping = {')': '(', '}': '{', ']': '['}

    for ch in s:
        # If we see a closing bracket...
        if ch in mapping:
            # If there's nothing to match it with, it's invalid
            if not stack:
                return False

            # The top of the stack must be the matching opening bracket
            if stack.pop() != mapping[ch]:
                return False

        # Otherwise it's an opening bracket, push it onto the stack
        else:
            stack.append(ch)

    # Valid only if no unmatched opening brackets remain
    return not stack


def isValid(s):
    stack = []
    mapping = {')':'(','}':'{',']':'['}

    for ch in s:
        if ch in mapping:
            if not stack:
                return False
            if stack.pop() != mapping[ch]:
                return False
        else:
            stack.append(ch)
    return not stack
            



###############################################################################
# 2) REVERSE STRING (STACK)
# Problem:
# Given a string s, return the reversed string.
#
# Pattern:
# - Push each character onto a stack
# - Pop to rebuild the string in reverse order
#
# Time Complexity: O(n)
# Space Complexity: O(n)
###############################################################################

def reverseString(s):
    stack = []
    result = []

    # Push all characters onto the stack
    for ch in s:
        stack.append(ch)

    # Pop characters to reverse order
    while stack:
        result.append(stack.pop())

    # Join list into a final string
    return "".join(result)

def reverseString(s):
    stack = []
    result = []

    for ch in s:
        stack.append(ch)

    for ch in s:
        result.append(stack.pop())

    return "".join(result)