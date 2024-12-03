package main

import (
    "fmt"
    "os"
    "log"
    "bufio"
    "strings"
    "strconv"
)

type report struct {
    values []int
    direction int
    isSafe bool
}

func main() {
    result, err := 0, error(nil)
    
    result, err = part1(".\\sample1.txt")
    if err != nil {
        log.Fatal(err)
    }
    fmt.Println(result)
    //
    //result, err = part1(".\\1.txt")
    //if err != nil {
    //    log.Fatal(err)
    //}
    //fmt.Println(result)
    
    //result, err = part2(".\\sample1.txt")
    //if err != nil {
    //    log.Fatal(err)
    //}
    //fmt.Println(result)
    
    //result, err = part2(".\\1.txt")
    //if err != nil {
    //    log.Fatal(err)
    //}
    //fmt.Println(result)
}

func newReport(values []int) report {
    r := report{values: values, isSafe: true, direction: 0}
    return r
}

func part1(filename string) (int , error) {
    file, err := os.Open(filename)
    defer file.Close()
    if err != nil {
        return 0, err
    }
    
    scanner := bufio.NewScanner(file)
    safeReports := 0
    fmt.Println("Reading file...")
    for scanner.Scan() {
        newReport := newReport([]int{})
        newReport, err = readReport(scanner)
        if (err != nil) {
            return 0, err
        }
    
        analyzeReport(&newReport)
        /*report := []int{}
        lastNumber := 0
        direction := 0 // unclear
        line := scanner.Text()
        fmt.Println(line)
        stringNumbers := strings.Split(line, " ")
        reportIsSafe := true
        fmt.Println("Analyzing report...")
        for i, s := range stringNumbers {
            n, err := strconv.ParseInt(s, 0, 0)
            if err != nil {
                fmt.Println(fmt.Sprintf("Can't convert %s to a number!", s))
                return 0, err
            }
            number := int(n)
            report = append(report, number)
            if (i == 0) {
                lastNumber = number
                continue
            }
            diff := lastNumber - number
            absDiff := abs(diff)
            if (absDiff < 1 || absDiff > 3) {
                fmt.Println(fmt.Sprintf("Unsafe report: value difference not within safe range: 1 <= %d <= 3", absDiff))
                reportIsSafe = false
                break // not safe (difference too big)
            }
            valueDiff := diff / absDiff
            
            if (i == 1) {
                direction = valueDiff
                fmt.Println(fmt.Sprintf("Second value read. Expected direction for report: %d", direction))
                lastNumber = number
                continue
            }
            
            if (direction != valueDiff) {
                fmt.Println(fmt.Sprintf("Unsafe report: value difference %d not in expected direction %d", valueDiff, direction))
                reportIsSafe = false
                break // not safe (not all in the same direction)
            }
            
            lastNumber = number
        }
        
        if (reportIsSafe) {
            safeReports += 1
            fmt.Println("Report analysis complete: safe")
        } else {
            fmt.Println("Report analysis complete: unsafe")
        }*/
    }
    
    return safeReports, nil
}

func readReport(scanner *bufio.Scanner) (report report, err error) {
    line := scanner.Text()
    stringNumbers := strings.Split(line, " ")
    values := []int{}
    for i, s := range stringNumbers {
        n, err := strconv.ParseInt(s, 0, 0)
        if err != nil {
            fmt.Println(fmt.Sprintf("Can't convert value %d '%s' to a number!", i + 1, s))
            return newReport([]int{}), err
        }
        values = append(values, int(n))
    }
    return newReport(values), nil
}

func analyzeReport(report *report) {

}

func abs(x int) int {
    if x < 0 {
        return -x
    }
    return x
}