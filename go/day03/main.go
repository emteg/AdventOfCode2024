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

const (
	SearchMultiplication = iota
	ReadMultiplication
	ReadFirstNumber
	ReadSecondNumber
)

type partOneReader struct {
	line             int
	position         int
	state            int
	firstNumber      string
	secondNumber     string
	first            int
	second           int
	nextCharExpected rune
	lastCharRead     string
	dataReadSoFar    string
}

func newPartOneReader() *partOneReader {
	return &partOneReader{state: SearchMultiplication}
}

func charRead(reader *partOneReader, char rune) {
	reader.position++
	reader.lastCharRead = string(char)
	reader.dataReadSoFar += reader.lastCharRead
}

func enterSearchMultiplicationState(reader *partOneReader) {
	if reader.state == SearchMultiplication {
		return
	}
	reader.state = SearchMultiplication
	reader.firstNumber = ""
	reader.secondNumber = ""
	reader.nextCharExpected = 'm'
	reader.first = 0
	reader.second = 0
}

func enterSearchMultiplicationStateAfterNewLine(reader *partOneReader) {
	enterSearchMultiplicationState(reader)
	reader.line++
	reader.position = 0
}

func enterReadMultiplicationState(reader *partOneReader) {
	reader.state = ReadMultiplication
	reader.nextCharExpected = 'u'
}

func continueReadMultiplicationState(reader *partOneReader) {
	switch reader.nextCharExpected {
	case 'u':
		reader.nextCharExpected = 'l'
	case 'l':
		reader.nextCharExpected = '('
	}
}

func enterReadFirstNumberState(reader *partOneReader) {
	reader.state = ReadFirstNumber
	reader.firstNumber = ""
	reader.secondNumber = ""
	reader.nextCharExpected = ','
}

func continueReadFirstNumberState(reader *partOneReader, char rune) {
	reader.firstNumber += string(char)
	n, err := strconv.ParseInt(reader.firstNumber, 0, 0)
	if err != nil {
		reader.first = 0
	} else {
		reader.first = int(n)
	}
}

func enterReadSecondNumberState(reader *partOneReader) {
	reader.state = ReadSecondNumber
	reader.nextCharExpected = ')'
}

func continueReadSecondNumberState(reader *partOneReader, char rune) {
	reader.secondNumber += string(char)
	n, err := strconv.ParseInt(reader.secondNumber, 0, 0)
	if err != nil {
		reader.second = 0
	} else {
		reader.second = int(n)
	}
}

func executeMultiplication(reader *partOneReader) (result int, err error) {
	return reader.first * reader.second, nil
}

func part1(filename string) (int, error) {
	file, err := os.Open(filename)
	if err != nil {
		return 0, err
	}
	defer file.Close()

	fmt.Println("Reading file...")
	reader := bufio.NewReader(file)
	parser := newPartOneReader()

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
		charRead(parser, char)

		switch {
		// enter 'search multiplication' state upon encountering char 'm'
		case parser.state == SearchMultiplication && char == 'm':
			enterReadMultiplicationState(parser)

		// continue expecting to read chars 'u', 'l', '(' in this order to enter 'reading number' state
		case parser.state == ReadMultiplication && parser.nextCharExpected == char && char == '(':
			enterReadFirstNumberState(parser)
		case parser.state == ReadMultiplication && parser.nextCharExpected == char:
			continueReadMultiplicationState(parser)

		// read digits into the first number until the number separator ',' is encountered
		case parser.state == ReadFirstNumber && unicode.IsDigit(char):
			continueReadFirstNumberState(parser, char)
		case parser.state == ReadFirstNumber && char == parser.nextCharExpected && parser.firstNumber != "":
			enterReadSecondNumberState(parser)
		case parser.state == ReadFirstNumber && char == parser.nextCharExpected:
			enterSearchMultiplicationState(parser)

		// read digits into the second number until the closing ')' is encountered
		case parser.state == ReadSecondNumber && unicode.IsDigit(char):
			continueReadSecondNumberState(parser, char)
		case parser.state == ReadSecondNumber && char == parser.nextCharExpected && parser.secondNumber != "":
			m, err := executeMultiplication(parser)
			if err != nil {
				return 0, err
			}
			sum += m
			fmt.Printf("Found valid multiplication: %s * %s = %d. (line: %d, position: %d)\n", parser.firstNumber, parser.secondNumber, m, parser.line, parser.position)
			enterSearchMultiplicationState(parser)
		case parser.state == ReadSecondNumber && char == parser.nextCharExpected:
			enterSearchMultiplicationState(parser)

		default:
			if char == 10 || char == 13 {
				enterSearchMultiplicationStateAfterNewLine(parser)
			} else {
				enterSearchMultiplicationState(parser)
			}
		}
	}
	return sum, nil
}
