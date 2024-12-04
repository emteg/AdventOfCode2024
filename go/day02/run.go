package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"strconv"
	"strings"
)

type report struct {
	values    []int
	skipped   int
	direction int
	isSafe    bool
}

func main() {
	result, err := 0, error(nil)

	result, err = execute(".\\sample1.txt", false)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(result)

	result, err = execute(".\\1.txt", false)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(result)

	result, err = execute(".\\sample1.txt", true)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(result)

	result, err = execute(".\\1.txt", true)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(result)
}

func newReport(values []int) report {
	r := report{values: values, skipped: -1, isSafe: true, direction: 0}
	return r
}

func execute(filename string, problemsDampened bool) (int, error) {
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
		if err != nil {
			return 0, err
		}

		if !problemsDampened {
			analyzeReport1(&newReport)
		} else {
			analyzeReport2(&newReport)
		}

		if newReport.isSafe {
			safeReports += 1
		}
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
			fmt.Println(fmt.Sprintf("Can't convert value %d '%s' to a number!", i+1, s))
			return newReport([]int{}), err
		}
		values = append(values, int(n))
	}
	return newReport(values), nil
}

func analyzeReport1(report *report) {
	fmt.Println("Analyzing report...")
	lastNumber := 0
	for i, number := range report.values {
		if i == 0 {
			lastNumber = number
			continue
		}

		diff := lastNumber - number
		absDiff := abs(diff)
		if absDiff < 1 || absDiff > 3 {
			fmt.Println(fmt.Sprintf("Unsafe report: value difference not within safe range: 1 <= %d <= 3", absDiff))
			report.isSafe = false
			break // not safe (difference too big)
		}

		valueDiff := diff / absDiff

		if i == 1 {
			report.direction = valueDiff
			fmt.Println(fmt.Sprintf("Second value read. Expected direction for report: %d", report.direction))
			lastNumber = number
			continue
		}

		if report.direction != valueDiff {
			fmt.Println(fmt.Sprintf("Unsafe report: value difference %d not in expected direction %d", valueDiff, report.direction))
			report.isSafe = false
			break // not safe (not all in the same direction)
		}

		lastNumber = number
	}

	if report.isSafe {
		fmt.Println("Report analysis complete: safe")
	} else {
		fmt.Println("Report analysis complete: unsafe")
	}
}

func analyzeReport2(report *report) {
	for report.skipped < len(report.values) {
		doAnalyzeReport2(report)
		if report.isSafe {
			return
		}

		report.skipped += 1
		fmt.Println(fmt.Sprintf("Report not safe. Trying to skip element %d...", report.skipped))
	}
	fmt.Println("Giving up, report isn't safe even with Problem Dampener enabled.")
}

func doAnalyzeReport2(report *report) {
	fmt.Println(fmt.Sprintf("Analyzing report %v...", report))
	lastNumber := -1
	report.isSafe = true
	report.direction = 0
	firstNumberRead := false
	secondNumberRead := false
	for i, number := range report.values {
		if report.skipped == i {
			fmt.Println(fmt.Sprintf("Skipping value %d", i))
			continue
		}

		if !firstNumberRead {
			lastNumber = number
			firstNumberRead = true
			continue
		}

		diff := lastNumber - number
		absDiff := abs(diff)
		if absDiff < 1 || absDiff > 3 {
			fmt.Println(fmt.Sprintf("Unsafe report: value difference between elements %d and %d not within safe range: 1 <= %d <= 3", i-1, i, absDiff))
			report.isSafe = false
			break // not safe (difference too big)
		}

		valueDiff := diff / absDiff

		if !secondNumberRead {
			report.direction = valueDiff
			//fmt.Println(fmt.Sprintf("Second value read. Expected direction for report: %d", report.direction))
			lastNumber = number
			secondNumberRead = true
			continue
		}

		if report.direction != valueDiff {
			fmt.Println(fmt.Sprintf("Unsafe report: value difference %d between elements %d and %d not in expected direction %d", valueDiff, i-1, i, report.direction))
			report.isSafe = false
			break // not safe (not all in the same direction)
		}

		lastNumber = number
	}
}

func abs(x int) int {
	if x < 0 {
		return -x
	}
	return x
}
