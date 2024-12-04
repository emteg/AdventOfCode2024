package main

import (
	"bufio"
	"fmt"
	"io"
	"log"
	"os"
	"strconv"
	"unicode"
)

func main() {
	sum, err := 0, error(nil)

	sum, err = part1("sample1.txt")
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(sum)

	sum, err = part1("1.txt")
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(sum)
}

func part1(filename string) (int, error) {
	file, err := os.Open(filename)
	defer file.Close()
	if err != nil {
		return 0, err
	}

	fmt.Println("Reading file...")
	reader := bufio.NewReader(file)
	isReadingMultiplication := false
	isReadingNumbers := false
	isReadingFirstNumber := false
	nextCharExpected := 'm'
	firstNumber := ""
	secondNumber := ""
	sum := 0
	for {
		char, _, err := reader.ReadRune()
		if err != nil {
			if err == io.EOF {
				break
			} else {
				log.Fatal(err)
			}
		}

		switch {
		// enter 'search multiplication' state upon encountering char 'm'
		case !isReadingMultiplication && char == 'm':
			fmt.Println("Found character 'm' - entering 'Read multiplication' state.")
			isReadingMultiplication = true
			nextCharExpected = 'u'

		// continue expecting to read chars 'u', 'l', '(' in this order to enter 'reading number' state
		case isReadingMultiplication && nextCharExpected == char && char == 'u':
			nextCharExpected = 'l'
		case isReadingMultiplication && nextCharExpected == char && char == 'l':
			nextCharExpected = '('
		case isReadingMultiplication && nextCharExpected == char && char == '(':
			fmt.Println("Found character '(' - entering 'Read first number' state.")
			isReadingNumbers = true
			isReadingFirstNumber = true
			firstNumber = ""
			secondNumber = ""
			nextCharExpected = ','

		// read digits into the first number until the number separator ',' is encountered
		case isReadingMultiplication && isReadingNumbers && isReadingFirstNumber && unicode.IsDigit(char):
			firstNumber += string(char)
		case isReadingMultiplication && isReadingNumbers && isReadingFirstNumber && char == ',' && firstNumber != "":
			fmt.Printf("Found character ',' - entering 'Read second number' state (first number was %s).\n", firstNumber)
			isReadingFirstNumber = false
			nextCharExpected = ')'
		case isReadingMultiplication && isReadingNumbers && isReadingFirstNumber && char == ',':
			fmt.Println("Found character ',' but the first number is empty - entering 'Search multiplication' state.")
			isReadingMultiplication = false
			isReadingNumbers = false
			firstNumber = ""
			secondNumber = ""
			nextCharExpected = 'm'

		// read digits into the second number until the closing ')' is encountered
		case isReadingMultiplication && isReadingNumbers && !isReadingFirstNumber && unicode.IsDigit(char):
			secondNumber += string(char)
		case isReadingMultiplication && isReadingNumbers && !isReadingFirstNumber && char == ')' && secondNumber != "":
			fmt.Printf("Found character ')' - finished reading multiplication (second number was %s).\n", secondNumber)
			a, _ := strconv.ParseInt(firstNumber, 0, 0)
			b, _ := strconv.ParseInt(secondNumber, 0, 0)
			multiplied := int(a) * int(b)
			fmt.Printf("Multiplication result was %d. Entering 'Search multiplication' state.\n", multiplied)
			sum += multiplied
			isReadingMultiplication = false
			isReadingNumbers = false
			isReadingFirstNumber = false
			firstNumber = ""
			secondNumber = ""
			nextCharExpected = 'm'
		case isReadingMultiplication && isReadingNumbers && !isReadingFirstNumber && char == ',':
			fmt.Println("Found character ',' but the second number is empty - entering 'Search multiplication' state.")
			isReadingMultiplication = false
			isReadingNumbers = false
			isReadingFirstNumber = false
			firstNumber = ""
			secondNumber = ""
			nextCharExpected = 'm'

		// reset state to 'search multiplication' state
		default:
			isReadingMultiplication = false
			isReadingNumbers = false
			isReadingFirstNumber = false
			firstNumber = ""
			secondNumber = ""
			nextCharExpected = 'm'
		}
	}
	return sum, nil
}
