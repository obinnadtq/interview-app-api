﻿using Application.Repositories;
using Domain.Models.ValueObject;
using Domain.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases.QuestionSet.GetQuestionSets;

public class GetQuestionSetsUseCase : IGetQuestionSetsUseCase
{
    private IOutputPort _outputPort;

    private readonly IQuestionSetRepository _questionSetRepository;
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionSetsUseCase(IQuestionSetRepository questionSetRepository, IQuestionRepository questionRepository)
    {
        _questionSetRepository = questionSetRepository;
        _questionRepository = questionRepository;
    }

    public async Task Execute(GetQuestionSetsInput input)
    {
        var questionSets = await _questionSetRepository.GetAll();

        if (questionSets is null || !questionSets.Any())
        {
            _outputPort.NotFound();
            return;
        }

        List<QuestionSetListItem> questionList = new List<QuestionSetListItem>();

        foreach(var questionSet in questionSets)
        {
            var questions = await _questionRepository.GetQuestionsBySetId(questionSet.Id);
            var averageDifficulty = questions.Select(q => q.Difficulty).Average() * 20;

            questionList.Add(new QuestionSetListItem()
            {
                QuestionSet = questionSet,
                Difficulty = new Difficulty { Value = averageDifficulty is null ? 0 : (int)averageDifficulty},
                Tags = questions.Select(q => new Category { Title = q.Category }).DistinctBy(c => c.Title)

            });
        }

        _outputPort.Ok(questionList);
    }

    public void SetOutputPort(IOutputPort outputPort) => _outputPort = outputPort;
}
