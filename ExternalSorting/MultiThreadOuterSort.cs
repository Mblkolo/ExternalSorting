using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting
{
    public class MultiThreadOuterSort
    {
        private string? _headers;
        private readonly int _chosenField;
        private long _iterations, _segments;
        private readonly Type[] _columnOfTableTypes =
            { typeof(int), typeof(string), typeof(DateTime) };

        public MultiThreadOuterSort(int chosenField)
        {
            _chosenField = chosenField;
            _iterations = 1;
        }

        private void SplitToFiles()
        {
            _segments = 1;
            using var fileA = new StreamReader("A.csv");
            _headers = fileA.ReadLine();

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");
            using var fileD = new StreamWriter("D.csv");
            string? currentRecord = fileA.ReadLine();
            //переменная flag поменяла свой тип с bool на int, т.к. теперь нам нужно больше
            //двух значений
            int flag = 0;
            int counter = 0;
            while (currentRecord is not null)
            {
                if (counter == _iterations)
                {
                    //Случай, когда мы дошли до конца цепочки
                    counter = 0;
                    flag = GetNextFileIndexToWrite(flag);
                    _segments++;
                }

                switch (flag)
                {
                    case 0:
                        fileB.WriteLine(currentRecord);
                        break;
                    case 1:
                        fileC.WriteLine(currentRecord);
                        break;
                    case 2:
                        fileD.WriteLine(currentRecord);
                        break;
                }

                currentRecord = fileA.ReadLine();
                counter++;
            }
        }

        //Метод получения следующего индекса файла для записи (B = 0, C = 1, D = 2)
        private static int GetNextFileIndexToWrite(int currentIndex)
                => currentIndex switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 0,
                    _ => throw new Exception("Что-то вышло из под контроля. Будем разбираться")
                };

        private void MergeFiles()
        {
            using var writerA = new StreamWriter("A.csv");

            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");
            using var readerD = new StreamReader("D.csv");

            writerA.WriteLine(_headers);

            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();
            string? elementD = readerD.ReadLine();

            int counterB = 0;
            int counterC = 0;
            int counterD = 0;
            while (elementB is not null || elementC is not null || elementD is not null)
            {
                string? currentRecord;
                int flag;

                if (CheckElement(elementB, counterB) && !CheckElement(elementC, counterC) && !CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочка закончилась только в файле B
                    (currentRecord, flag) = GetMinOfElements(
                            elementC,
                            elementD) switch
                    {
                        0 => (elementC, 1),
                        1 => (elementD, 2)
                    };
                }
                else if (CheckElement(elementC, counterC) && !CheckElement(elementB, counterB) && !CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочка закончилась только в файле С
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementD) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementD, 2)
                    };
                }
                else if (CheckElement(elementD, counterD) && !CheckElement(elementB, counterB) && !CheckElement(elementC, counterC))
                {
                    //Случай, когда цепочка закончилась только в файле D
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementC) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementC, 1)
                    };
                }
                else if (counterB == _iterations && counterC == _iterations)
                {
                    //Случай, когда цепочки закончились в файлах В и С
                    currentRecord = elementD;
                    flag = 2;
                }
                else if (counterB == _iterations && counterD == _iterations)
                {
                    //Случай, когда цепочки закончились в файлах В и D
                    currentRecord = elementC;
                    flag = 1;
                }
                else if (counterC == _iterations && counterD == _iterations)
                {
                    //Случай, когда цепочки закончились в файлах C и D
                    currentRecord = elementB;
                    flag = 0;
                }
                else
                {
                    //Случай, когда не закончилась ни одна из 3 цепочек
                    (currentRecord, flag) = GetMinOfElements(
                            elementB,
                            elementC,
                            elementD) switch
                    {
                        0 => (elementB, 0),
                        1 => (elementC, 1),
                        2 => (elementD, 2)
                    };
                }

                switch (flag)
                {
                    case 0:
                        writerA.WriteLine(currentRecord);
                        elementB = readerB.ReadLine();
                        counterB++;
                        break;
                    case 1:
                        writerA.WriteLine(currentRecord);
                        elementC = readerC.ReadLine();
                        counterC++;
                        break;
                    case 2:
                        writerA.WriteLine(currentRecord);
                        elementD = readerD.ReadLine();
                        counterD++;
                        break;
                }

                if (counterB != _iterations || counterC != _iterations || counterD != _iterations)
                {
                    continue;
                }

                //Обнуляем все 3 счётчика, если достигли конца всех цепочек во всех файлах
                counterC = 0;
                counterB = 0;
                counterD = 0;
            }

            _iterations *= 3;
        }

        //Ниже дан ряд методов для поиска минимального из 3 элементов (с учётом того),
        //что некоторые из них могут отсутствовать
        private int GetMinOfElements(params string?[] elements)
        {
            if (elements.Contains(null))
            {
                switch (elements.Length)
                {
                    case 2:
                        return elements[0] is null ? 1 : 0;
                    case 3 when elements[0] is null && elements[1] is null:
                        return 2;
                    case 3 when elements[0] is null && elements[2] is null:
                        return 1;
                    case 3 when elements[1] is null && elements[2] is null:
                        return 0;
                }
            }

            if (_columnOfTableTypes[_chosenField].IsEquivalentTo(typeof(int)))
            {
                return GetMinInt(elements
                    .Select(s => s is null ? int.MaxValue : int.Parse(s.Split(';')[_chosenField]))
                    .ToArray());
            }
            if (_columnOfTableTypes[_chosenField].IsEquivalentTo(typeof(DateTime)))
            {
                return GetMinDateTime(elements
                    .Select(s => s is null ? DateTime.MaxValue : DateTime.Parse(s.Split(';')[_chosenField]))
                    .ToArray());
            }

            return GetMinString(elements!);
        }

        private int GetMinString(IReadOnlyList<string> elements)
        {
            if (elements.Count == 1)
            {
                return 0;
            }

            var min = elements[0].Split(';')[_chosenField];
            var minIndex = 0;
            for (var i = 1; i < elements.Count; i++)
            {
                if (string.Compare(elements[i].Split(';')[_chosenField], min, StringComparison.Ordinal) > 0)
                {
                    continue;
                }

                min = elements[i].Split(';')[_chosenField];
                minIndex = i;
            }

            return minIndex;
        }

        private static int GetMinInt(IReadOnlyList<int> elements)
        {
            if (elements.Count == 1)
            {
                return 0;
            }

            var min = elements[0];
            var minIndex = 0;
            for (var i = 1; i < elements.Count; i++)
            {
                if (elements[i] > min)
                {
                    continue;
                }

                min = elements[i];
                minIndex = i;
            }

            return minIndex;
        }

        private static int GetMinDateTime(IReadOnlyList<DateTime> elements)
        {
            if (elements.Count == 1)
            {
                return 0;
            }

            var min = elements[0];
            var minIndex = 0;
            for (var i = 1; i < elements.Count; i++)
            {
                if (DateTime.Compare(elements[i], min) > 0)
                {
                    continue;
                }

                min = elements[i];
                minIndex = i;
            }

            return minIndex;
        }

        private bool CheckElement(string? element, int counter)
            => element is null || counter == _iterations;

        public void Sort()
        {
            while (true)
            {
                SplitToFiles();

                if (_segments == 1)
                {
                    break;
                }

                MergeFiles();
            }
        }
    }
}
