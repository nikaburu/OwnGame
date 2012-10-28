﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OwnGame.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace OwnGame.Servicies
{
    public class ExcelDataQuestionService: IQuestionService
    {
        #region Implementation of IQuestionService
        private readonly Random _random = new Random();
        private readonly string _folder = Environment.CurrentDirectory + @"\Servicies\Questions\";
        private const string Level1FileName = "level1.xls";
        private const string Level2FileName = "level2.xls";
        private const string Level3FileName = "level3.xls";
        private Excel.Application _application;


        public List<QuestionGroup> GetQuestionGroupList(int multi = 1)
        {
            var list = new List<QuestionGroup>();
            _application = new Excel.Application();
           // application.Workbooks.Open(Level1FileName);
            _application.Visible = false;
            switch (multi)
            {
                case 1:
                    _application.Workbooks.Open(_folder+Level1FileName);       
                    break;
                case 2:
                    _application.Workbooks.Open(_folder + Level2FileName);
                    break;
                case 3:
                    _application.Workbooks.Open(_folder + Level3FileName);
                    break;
            }

            foreach (Excel.Worksheet sheet in _application.Workbooks[1].Worksheets)
            {
                
                            
                var i = 1;
                var questions = new List<Question>();
                var columns = (Excel.Range) sheet.Rows[i];
                while (((Excel.Range)columns.Columns[1]).Value2 != null && !String.IsNullOrEmpty( ((Excel.Range)columns.Columns[1]).Value2.ToString()))
                {
                    questions.Add( new Question
                                       {
                                           Id = _random.Next(int.MaxValue),
                                           Text = ((Excel.Range)columns.Columns[1]).Value2.ToString(),
                                           TextImage = columns.Columns[2] ==null ? null : File.ReadAllBytes(_folder+((Excel.Range)columns.Columns[2]).Value2.ToString()),
                                           Answer = ((Excel.Range)columns.Columns[3]).Value2.ToString(),
                                           AnswerImage = String.IsNullOrEmpty(((Excel.Range)columns.Columns[4]).Value2.ToString()) ? null : File.ReadAllBytes(_folder + ((Excel.Range)columns.Columns[4]).Value2.ToString()),
                                           Cost = int.Parse(((Excel.Range)columns.Columns[5]).Value2.ToString())
                                       });
                    i++;
                    columns = (Excel.Range)sheet.Rows[i];
                }
                var group = new QuestionGroup(_random.Next(int.MaxValue), GetRandomQuestionsByPrice(questions));
                group.Name = sheet.Name;  
                list.Add(group);
            }

            _application.Workbooks[1].Close();
            Marshal.ReleaseComObject(_application);
            return GetRandomGroups(list).ToList();
        }

        private IEnumerable<Question> GetRandomQuestionsByPrice(List<Question> questions)
        {
            var questionsByPrice = new Dictionary<int, List<Question>>();
            foreach (var question in questions)
            {
                if(questionsByPrice.ContainsKey(question.Cost))
                {
                    questionsByPrice[question.Cost].Add(question);
                }
                else
                {
                    questionsByPrice.Add(question.Cost, new List<Question>{question});
                }
            }

            return questionsByPrice.Select(priceGroup => RandomPermutation(priceGroup.Value).First()).ToList();

        }

        private IEnumerable<QuestionGroup> GetRandomGroups(IEnumerable<QuestionGroup> groups )
        {
            return RandomPermutation(groups);
        }


        static readonly Random Random = new Random();

        public static IEnumerable<T> RandomPermutation<T>(IEnumerable<T> sequence)
        {
            T[] retArray = sequence.ToArray();
            for (int i = 0; i < retArray.Length - 1; i += 1)
            {
                int swapIndex = Random.Next(i + 1, retArray.Length);
                T temp = retArray[i];
                retArray[i] = retArray[swapIndex];
                retArray[swapIndex] = temp;
            }

            return retArray.Take(5).ToList();
        }

        #endregion
    }
}
