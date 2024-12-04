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

	sum, err = part1("sample1.txt", false)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(sum)
	//
	//sum, err = part1("1.txt", false)
	//if err != nil {
	//log.Fatal(err)
	//}
	//fmt.Println(sum)

	//sum, err = part1("sample2.txt", true)
	//if err != nil {
	//	log.Fatal(err)
	//}
	//fmt.Println(sum)
}

// char types
const (
	Other = iota
	CharD
	CharL
	CharM
	CharN
	CharO
	CharU
	CharT
	Apostrophe
	OpenBraces
	CloseBraces
	Digit
	Comma
)

func charType(char rune) int {
	switch {
	case char == 'd':
		return CharD
	case char == 'l':
		return CharL
	case char == 'm':
		return CharM
	case char == 'n':
		return CharN
	case char == 'o':
		return CharO
	case char == 'u':
		return CharU
	case char == 't':
		return CharT
	case char == '`':
		return Apostrophe
	case char == '(':
		return OpenBraces
	case char == ')':
		return CloseBraces
	case unicode.IsDigit(char):
		return Digit
	case char == ',':
		return Comma
	default:
		return Other
	}
}

func matchesExpectedChar(actual rune, expected ...rune) bool {
	for _, e := range expected {
		if actual == e {
			return true
		}
	}
	return false
}

// parser states
const (
	SearchInstruction = iota
	ReadMultiplication
	ReadFirstNumber
	ReadSecondNumber
	ReadDoOrDont
	ReadDo
	ReadDont
	ReadOpenCloseBraces
)

type partOneReader struct {
	line                  int
	position              int
	state                 int
	firstNumber           string
	secondNumber          string
	first                 int
	second                int
	nextChars             []int
	nextCharExpected      rune
	lastCharRead          string
	dataReadSoFar         string
	withConditionals      bool
	multiplicationEnabled bool
}

func newPartOneReader(withConditionals bool) *partOneReader {
	var nextChars []int
	if withConditionals {
		nextChars = append(nextChars, CharM, CharD) // looking for 'mul', 'do' and 'don`t'
	} else {
		nextChars = append(nextChars, CharM) // only looking for 'mul'
	}
	return &partOneReader{
		state:                 SearchInstruction,
		withConditionals:      withConditionals,
		multiplicationEnabled: true,
		nextChars:             nextChars,
	}
}

func charRead(reader *partOneReader, char rune) {
	reader.position++
	reader.lastCharRead = string(char)
	reader.dataReadSoFar += reader.lastCharRead
}

func enterSearchInstructionState(reader *partOneReader) {
	if reader.state == SearchInstruction {
		return
	}
	reader.state = SearchInstruction
	reader.firstNumber = ""
	reader.secondNumber = ""
	if reader.withConditionals {
		reader.nextChars = []int{CharM, CharD} // looking for 'mul', 'do' and 'don`t'
	} else {
		reader.nextChars = []int{CharM} // only looking for 'mul'
	}
	reader.nextCharExpected = 'm'
	reader.first = 0
	reader.second = 0
}

func enterSearchInstructionStateAfterNewLine(reader *partOneReader) {
	enterSearchInstructionState(reader)
	reader.line++
	reader.position = 0
}

func enterReadMultiplicationState(reader *partOneReader) {
	reader.state = ReadMultiplication
	reader.nextCharExpected = 'u'
	reader.nextChars = []int{CharU}
}

func continueReadMultiplicationState(reader *partOneReader) {
	switch reader.nextCharExpected {
	case 'u':
		reader.nextCharExpected = 'l'
		reader.nextChars = []int{CharL}
	case 'l':
		reader.nextCharExpected = '('
		reader.nextChars = []int{OpenBraces}
	}
}

func enterReadFirstNumberState(reader *partOneReader) {
	reader.state = ReadFirstNumber
	reader.firstNumber = ""
	reader.secondNumber = ""
	reader.nextCharExpected = ','
	reader.nextChars = []int{Digit, Comma}
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
	reader.nextChars = []int{Digit, CloseBraces}
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

func executeMultiplication(reader *partOneReader) int {
	return reader.first * reader.second
}

func enterReadDoOrDontState(reader *partOneReader) {
	reader.state = ReadDoOrDont
	reader.firstNumber = ""
	reader.secondNumber = ""
	reader.nextCharExpected = 'o'
	reader.nextChars = []int{CharO}
	reader.first = 0
	reader.second = 0
}

func enterReadDoState(reader *partOneReader) {
	reader.state = ReadDo
	reader.nextCharExpected = '('
}

func enterReadDontState(reader *partOneReader) {
	reader.state = ReadDont
	reader.nextCharExpected = 'n'
}

func continueReadDontState(reader *partOneReader, char rune) {
	switch reader.nextCharExpected {
	case 'n':
		reader.nextCharExpected = '`'
	case '`':
		reader.nextCharExpected = 't'
	case 't':
		reader.nextCharExpected = '('
	case ')':
		reader.nextCharExpected = ')'
	}
}

func finishDoOrDontState(reader *partOneReader) {
	if reader.state == ReadDo {
		reader.multiplicationEnabled = true
	} else if reader.state == ReadDont {
		reader.multiplicationEnabled = false
	}
}

func part1(filename string, withConditionals bool) (int, error) {
	file, err := os.Open(filename)
	if err != nil {
		return 0, err
	}
	defer file.Close()

	fmt.Println("Reading file...")
	reader := bufio.NewReader(file)
	parser := newPartOneReader(withConditionals)

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
		// enter 'read multiplication' state upon encountering char 'm'
		case parser.state == SearchInstruction && char == 'm':
			enterReadMultiplicationState(parser)
		// enter 'read do or dont' state upon encountering char 'm'
		case parser.state == SearchInstruction && parser.withConditionals && char == 'd':
			enterReadDoOrDontState(parser)

		// continue expecting to read chars 'u', 'l', '(' in this order to enter 'reading number' state
		case parser.state == ReadMultiplication && parser.nextCharExpected == char && char == '(':
			enterReadFirstNumberState(parser)
		case parser.state == ReadMultiplication && parser.nextCharExpected == char:
			continueReadMultiplicationState(parser)

		// expect char 'o' in 'read do or dont' state
		case parser.state == ReadDoOrDont && parser.nextCharExpected == char && char == 'o':
			continue

		// enter 'read do' state upon encountering char '(' in 'read do or dont' state
		case parser.state == ReadDoOrDont && parser.nextCharExpected == 'o' && char == '(':
			enterReadDoState(parser)

		// enter 'read dont' state upon encountering char 'n' in 'read do or dont' state
		case parser.state == ReadDoOrDont && parser.nextCharExpected == 'o' && char == 'n':
			enterReadDontState(parser)

		case parser.state == ReadDo && parser.nextCharExpected == char:
			finishDoOrDontState(parser)
			enterSearchInstructionState(parser)

		case parser.state == ReadDont && parser.nextCharExpected == char && char != ')':
			continueReadDontState(parser, char)
		case parser.state == ReadDont && parser.nextCharExpected == char:
			finishDoOrDontState(parser)
			enterSearchInstructionState(parser)

		// read digits into the first number until the number separator ',' is encountered
		case parser.state == ReadFirstNumber && unicode.IsDigit(char):
			continueReadFirstNumberState(parser, char)
		case parser.state == ReadFirstNumber && char == parser.nextCharExpected && parser.firstNumber != "":
			enterReadSecondNumberState(parser)
		case parser.state == ReadFirstNumber && char == parser.nextCharExpected:
			enterSearchInstructionState(parser)

		// read digits into the second number until the closing ')' is encountered
		case parser.state == ReadSecondNumber && unicode.IsDigit(char):
			continueReadSecondNumberState(parser, char)
		case parser.state == ReadSecondNumber && char == parser.nextCharExpected && parser.secondNumber != "":
			m := executeMultiplication(parser)
			sum += m
			fmt.Printf("Found valid multiplication: %s * %s = %d. (line: %d, position: %d)\n", parser.firstNumber, parser.secondNumber, m, parser.line, parser.position)
			enterSearchInstructionState(parser)
		case parser.state == ReadSecondNumber && char == parser.nextCharExpected:
			enterSearchInstructionState(parser)

		default:
			if char == 10 || char == 13 {
				enterSearchInstructionStateAfterNewLine(parser)
			} else {
				enterSearchInstructionState(parser)
			}
		}
	}
	return sum, nil
}
