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
    file, err := os.Open(".\\1.txt")
    if err != nil {
        log.Fatal(err)
    }
    defer file.Close()
    
    leftList := []int{};
    rightList := []int{};
    
    scanner := bufio.NewScanner(file)
    for scanner.Scan() {
        line := scanner.Text()
        stringNumbers := strings.Split(line, "   ")
        
        left, err := strconv.ParseInt(stringNumbers[0], 0, 0)
        if err != nil {
            log.Fatal(err)
        }
        right, err := strconv.ParseInt(stringNumbers[1], 0, 0)
        if err != nil {
            log.Fatal(err)
        }
        
        leftList = append(leftList, int(left))
        rightList = append(rightList, int(right))
    }
    if err := scanner.Err(); err != nil {
        log.Fatal(err)
    }
    
    sort.Ints(leftList)
    sort.Ints(rightList)
    
    distances := []int{}
    sum := 0
    for index, element := range leftList {
        distance := abs(element - rightList[index])
        distances = append(distances, distance)
        sum += distance
    }
    
    fmt.Println(sum)
}

func abs(x int) int {
    if x < 0 {
        return -x
    }
    return x
}