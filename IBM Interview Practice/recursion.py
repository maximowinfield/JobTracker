def fact(n):
    # assuming that n is a positive integer or 0
    if n >= 1:
        # we call the function on itself by calculating the factorial this way
        # a better way to say this is: recursive case: multiply n by the
        # factorial of n-1
        return n * fact(n - 1)
    else:
        # this is the base case: factorial of 0 is 1
        return 1

print("0! =", fact(0)) # 1
print("1! =", fact(1)) # 1 * fact(1-1) = 1 * fact(0)
print("2! =", fact(2)) # 2 * fact(2-1) = 2 * fact(1)
print("3! =", fact(3)) # 3 * fact(3-1) = 3 * fact(2)
print("4! =", fact(4)) # 4 * fact(4-1) = 4 * fact(3)

#it's important to note we are getting the factorials of each previous factorial before
#calculating the factorial. For example fact(3-1) = fact(2) which is calculated as
# 2 * fact(1) = 2 and fact(1) is calculated as 1 * fact(0) which is 1



def fact(n):
    # Assume n is a non-negative integer
    if n >= 1:
        # Recursive case: multiply n by the factorial of (n - 1)
        return n * fact(n - 1)
    else:
        # Base case: factorial of 0 is 1
        return 1

print("0! =", fact(0))  # 1
print("1! =", fact(1))  # 1 * fact(0) = 1
print("2! =", fact(2))  # 2 * fact(1) = 2
print("3! =", fact(3))  # 3 * fact(2) = 6
print("4! =", fact(4))  # 4 * fact(3) = 24

# Note:
# Recursion works by breaking the problem into smaller subproblems.
# Each call waits for the result of the next call until the base case (fact(0)) is reached.
# Then the results are returned back up the call stack.

# ===========================================================
# ===========================================================
# ===========================================================

# The fibonacci sequence is defined as follows
# F of n = F of n-1 + F of n-2 (if n >= 3) (If less than 3, those fibs are
# implicitly set as 1 in the function. This avoids setting them explicitly.)
# otherwise (else)
# it equals 1

#define the fibonacci function and pass a number through it
def fib(n):
    # assuming that n is a positive integer
    # this means: If n is greater than or equal to 3 then return the sum of
    # the two previous fibonacci numbers
    if n >= 3:
        return fib(n-1) + fib(n-2)
    # otherwise return 1
    else:
        # we are defining fib(1) and fib(2) implicitly by returning 1
        # if the fib is less than 3
        return 1

print("fib(1) =", fib(1)) # 1 is not greater than 3 therefore it returns 1
print("fib(2) =", fib(2)) # 2 is not greater than 3 therefore it returns 1
print("fib(3) =", fib(3)) # 3 equals 3 therefore it returns fib(2) + fib(1) which is
                          # 1 + 1
print("fib(4) =", fib(4)) # returns fib(3) + fib(2) 
print("fib(5) =", fib(5)) # returns fib(4) + fib(3)

# Time Complexity: O(2^n)
# Each call to fib(n) makes two recursive calls: fib(n-1) and fib(n-2).
# This leads to repeated calculations of the same subproblems,
# causing the number of function calls to grow exponentially as n increases.

# Space Complexity: O(n)
# The maximum depth of the recursion stack is n.
# Each recursive call remains on the call stack until it returns,
# so the space used grows linearly with n.
