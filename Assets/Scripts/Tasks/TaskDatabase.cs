using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "TaskDatabase",
    menuName = "Puzzle/Tasks/Task Database"
)]
public class TaskDatabase :
    ScriptableObject
{
    // =====================================================
    // TASKS
    // =====================================================

    [SerializeField]
    private List<TaskDefinition> tasks =
        new List<TaskDefinition>();


    public IReadOnlyList<TaskDefinition> Tasks =>
        tasks;


    // =====================================================
    // VALIDATION
    // =====================================================

    public bool IsValid(
        out string errorMessage
    )
    {
        if (tasks == null)
        {
            errorMessage =
                "Task list is null.";

            return false;
        }


        HashSet<string> usedIds =
            new HashSet<string>();


        for (
            int i = 0;
            i < tasks.Count;
            i++
        )
        {
            TaskDefinition task =
                tasks[i];


            if (task == null)
            {
                errorMessage =
                    $"Task at index {i} is null.";

                return false;
            }


            if (
                !task.IsValid(
                    out string taskError
                )
            )
            {
                errorMessage =
                    $"Task at index {i}: " +
                    taskError;

                return false;
            }


            if (
                !usedIds.Add(
                    task.Id
                )
            )
            {
                errorMessage =
                    $"Duplicate Task ID: " +
                    $"{task.Id}";

                return false;
            }
        }


        errorMessage =
            "";

        return true;
    }
}