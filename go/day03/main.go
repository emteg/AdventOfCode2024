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

func toCharType(char rune) int {
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
	case char == '\'':
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

// parser states
const (
	SearchInstruction = iota
	ReadMul
	ReadFirstNumber
	ReadSecondNumber
	ReadDoOrDont
	ReadDo
	ReadDont
)

type memoryReader struct {
	line                   int
	position               int
	state                  int
	currentNumber          string
	lastNumber             int
	nextChars              []int
	lastCharRead           string
	dataReadSoFar          string
	withConditionals       bool
	multiplicationsEnabled bool
}

func newMemoryReaderWithoutConditionals() *memoryReader {
	return &memoryReader{
		state:                  SearchInstruction,
		withConditionals:       false,
		multiplicationsEnabled: true,
		nextChars:              []int{CharM},
	}
}

func newMemoryReaderWithConditionals() *memoryReader {
	return &memoryReader{
		state:                  SearchInstruction,
		withConditionals:       true,
		multiplicationsEnabled: true,
		nextChars:              []int{CharM, CharD},
	}
}

func readNext(reader *memoryReader, char rune) int {
	reader.position++
	if char == 10 || char == 13 {
		reader.position = 0
		reader.line++
	}
	reader.lastCharRead = string(char)
	reader.dataReadSoFar += reader.lastCharRead
	charIsExpected := matchesExpectedChar(reader, char)
	switch {
	case reader.state == SearchInstruction && charIsExpected && char == 'm':
		enterReadMulState(reader)
		return 0
	case reader.state == SearchInstruction && charIsExpected && char == 'd':
		enterReadDoOrDontState(reader)
		return 0
	case reader.state == ReadMul && charIsExpected:
		continueReadMul(reader, char)
		return 0
	case (reader.state == ReadDoOrDont || reader.state == ReadDo || reader.state == ReadDont) && charIsExpected:
		continueReadDoOrDont(reader, char)
		return 0
	case (reader.state == ReadFirstNumber || reader.state == ReadSecondNumber) && charIsExpected:
		return continueReadNumber(reader, char)
	default:
		enterOrContinueSearchInstructionState(reader)
		return 0
	}
}

func enterOrContinueSearchInstructionState(reader *memoryReader) {
	if reader.state == SearchInstruction {
		return
	}

	reader.state = SearchInstruction
	reader.currentNumber = ""
	reader.lastNumber = 0
	if reader.withConditionals && reader.multiplicationsEnabled {
		reader.nextChars = []int{CharM, CharD}
	} else if reader.withConditionals {
		reader.nextChars = []int{CharD}
	} else {
		reader.nextChars = []int{CharM}
	}
}

func enterReadMulState(reader *memoryReader) {
	reader.state = ReadMul
	expect(reader, CharU)
}

func continueReadMul(reader *memoryReader, char rune) {
	charType := toCharType(char)
	switch charType {
	case CharU:
		expect(reader, CharL)
	case CharL:
		expect(reader, OpenBraces)
	case OpenBraces:
		enterReadFirstNumberState(reader)
	}
}

func enterReadFirstNumberState(reader *memoryReader) {
	reader.state = ReadFirstNumber
	reader.currentNumber = ""
	reader.lastNumber = 0
	reader.nextChars = []int{Digit}
}

func enterReadSecondNumberState(reader *memoryReader, number int) {
	reader.lastNumber = number
	reader.nextChars = []int{Digit}
	reader.state = ReadSecondNumber
	reader.currentNumber = ""
}

func continueReadNumber(reader *memoryReader, char rune) int {
	currentChar := toCharType(char)
	switch {
	case reader.state == ReadFirstNumber && reader.currentNumber == "":
		reader.currentNumber = string(char)
		reader.nextChars = []int{Digit, Comma}
		return 0
	case reader.state == ReadSecondNumber && reader.currentNumber == "":
		reader.currentNumber = string(char)
		reader.nextChars = []int{Digit, CloseBraces}
		return 0
	case (reader.state == ReadFirstNumber || reader.state == ReadSecondNumber) && currentChar == Digit:
		reader.currentNumber += string(char)
		return 0
	case reader.state == ReadFirstNumber && currentChar == Comma:
		n, _ := strconv.ParseInt(reader.currentNumber, 0, 0)
		enterReadSecondNumberState(reader, int(n))
		return 0
	case reader.state == ReadSecondNumber && currentChar == CloseBraces:
		secondNumber, _ := strconv.ParseInt(reader.currentNumber, 0, 0)
		multiplicationResult := reader.lastNumber * int(secondNumber)
		enterOrContinueSearchInstructionState(reader)
		return multiplicationResult
	default:
		return 0
	}
}

func enterReadDoOrDontState(reader *memoryReader) {
	reader.state = ReadDoOrDont
	expect(reader, CharO)
}

func continueReadDoOrDont(reader *memoryReader, char rune) {
	currentChar := toCharType(char)
	switch {
	case reader.state == ReadDoOrDont && currentChar == CharO:
		expect(reader, OpenBraces, CharN)
	case reader.state == ReadDoOrDont && currentChar == OpenBraces:
		enterReadDoState(reader)
	case reader.state == ReadDoOrDont && currentChar == CharN:
		enterReadDontState(reader)
	case reader.state == ReadDo:
		reader.multiplicationsEnabled = true
		enterOrContinueSearchInstructionState(reader)
	case reader.state == ReadDont && currentChar == Apostrophe:
		expect(reader, CharT)
	case reader.state == ReadDont && currentChar == CharT:
		expect(reader, OpenBraces)
	case reader.state == ReadDont && currentChar == OpenBraces:
		expect(reader, CloseBraces)
	case reader.state == ReadDont && currentChar == CloseBraces:
		reader.multiplicationsEnabled = false
		enterOrContinueSearchInstructionState(reader)
	}
}

func enterReadDoState(reader *memoryReader) {
	reader.state = ReadDo
	expect(reader, CloseBraces)
}

func enterReadDontState(reader *memoryReader) {
	reader.state = ReadDont
	expect(reader, Apostrophe)
}

func expect(reader *memoryReader, chars ...int) {
	reader.nextChars = []int{}
	for _, char := range chars {
		reader.nextChars = append(reader.nextChars, char)
	}
}

func matchesExpectedChar(reader *memoryReader, actual rune) bool {
	for _, expected := range reader.nextChars {
		if toCharType(actual) == expected {
			return true
		}
	}
	return false
}

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

	sum, err = part2("sample2.txt")
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(sum)

	sum, err = part2("1.txt")
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(sum)
}

func part1(filename string) (int, error) {
	return execute(filename, newMemoryReaderWithoutConditionals())
}

func part2(filename string) (int, error) {
	return execute(filename, newMemoryReaderWithConditionals())
}

func execute(filename string, reader *memoryReader) (int, error) {
	file, err := os.Open(filename)
	if err != nil {
		return 0, err
	}
	defer file.Close()

	fmt.Println("Reading file...")
	fileReader := bufio.NewReader(file)
	sum := 0
	for {
		char, _, err := fileReader.ReadRune()
		if err != nil {
			if err == io.EOF {
				break
			} else {
				log.Fatal(err)
			}
		}

		sum += readNext(reader, char)
	}
	return sum, nil
}
