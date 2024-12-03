package main

import (
    "fmt"
    "os"
    "log"
    "bufio"
    "strings"
    "strconv"
    "sort"
)

func main() {
    result, err := 0, error(nil)
    
    result, err = part1(".\\sample1.txt")
    if err != nil {
        log.Fatal(err)
    }
    fmt.Println(result)
    
    result, err = part1(".\\1.txt")
    if err != nil {
        log.Fatal(err)
    }
    fmt.Println(result)
    
    result, err = part2(".\\sample1.txt")
    if err != nil {
        log.Fatal(err)
    }
    fmt.Println(result)
    
    result, err = part2(".\\1.txt")
    if err != nil {
        log.Fatal(err)
    }
    fmt.Println(result)
}

func part1(filename string) (int, error) {
    leftList, rightList, err := readFileIntoLists(filename)
    if err != nil {
        return 0, err
    }
    
    sort.Ints(leftList)
    sort.Ints(rightList)
    
    sum := 0
    for index, element := range leftList {
        distance := abs(element - rightList[index])
        sum += distance
    }
    
    return sum, nil
}

func part2(filename string) (int, error) {
    leftList, rightList, err := readFileIntoLists(filename)
    if err != nil {
        return 0, err
    }
    
    occurancesOfNumberInRightList := make(map[int]int)
    sum := 0
    similarity := 0
    
    for _, leftElement := range leftList {
        occurancesInRightList, exists := occurancesOfNumberInRightList[leftElement]
        if (!exists) {
            occurancesInRightList = 0
            
            for _, rightElement := range rightList {
                if (leftElement != rightElement) {
                    continue
                }
                occurancesInRightList += 1
            }
            
            occurancesOfNumberInRightList[leftElement] = occurancesInRightList
        }
        
        similarity = leftElement * occurancesInRightList
        sum += similarity
    }
    
    return sum, nil
}

func readFileIntoLists(filename string) (leftList []int, rightList []int, err error) {
    leftList = []int{};
    rightList = []int{};
    file, err := os.Open(filename)
    defer file.Close()
    if err != nil {
        return leftList, rightList, err
    }
    
    scanner := bufio.NewScanner(file)
    for scanner.Scan() {
        line := scanner.Text()
        stringNumbers := strings.Split(line, "   ")
        
        left, err := strconv.ParseInt(stringNumbers[0], 0, 0)
        if err != nil {
            return leftList, rightList, err
        }
        right, err := strconv.ParseInt(stringNumbers[1], 0, 0)
        if err != nil {
            return leftList, rightList, err
        }
        
        leftList = append(leftList, int(left))
        rightList = append(rightList, int(right))
    }
    if err := scanner.Err(); err != nil {
        return leftList, rightList, err
    }
    
    return leftList, rightList, nil
}

func abs(x int) int {
    if x < 0 {
        return -x
    }
    return x
}