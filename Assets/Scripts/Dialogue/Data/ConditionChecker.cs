using System.Collections.Generic;
using UnityEngine;

public static class ConditionChecker
{
    // =========================
    // FLAGS
    // =========================

    public static bool HasRequiredFlags(
        List<string> requiredFlags)
    {
        if (requiredFlags == null)
            return true;

        foreach (string flag in requiredFlags)
        {
            if (!GameFlags.Instance.HasFlag(flag))
            {
                return false;
            }
        }

        return true;
    }

    // =========================
    // BLOCKED FLAGS
    // =========================

    public static bool HasBlockedFlags(
        List<string> blockedFlags)
    {
        if (blockedFlags == null)
            return false;

        foreach (string flag in blockedFlags)
        {
            if (GameFlags.Instance.HasFlag(flag))
            {
                return true;
            }
        }

        return false;
    }

    // =========================
    // NODE CONDITIONS
    // =========================

    public static bool CanEnterNode(
        DialogueNode node,
        RelationshipSystem relationships)
    {
        if (!HasRequiredFlags(
            node.requiredFlags))
        {
            return false;
        }

        if (HasBlockedFlags(
            node.blockedFlags))
        {
            return false;
        }

        if (relationships.sadako
            < node.requiredSadako)
        {
            return false;
        }

        if (relationships.sumiko
            < node.requiredSumiko)
        {
            return false;
        }

        if (relationships.teruko
            < node.requiredTeruko)
        {
            return false;
        }

        return true;
    }

    // =========================
    // CHOICE CONDITIONS
    // =========================

    public static bool IsChoiceAvailable(
        DialogueChoice choice,
        RelationshipSystem relationships)
    {
        if (!HasRequiredFlags(
            choice.requiredFlags))
        {
            return false;
        }

        if (HasBlockedFlags(
            choice.blockedFlags))
        {
            return false;
        }

        if (relationships.sadako
            < choice.requiredSadako)
        {
            return false;
        }

        if (relationships.sumiko
            < choice.requiredSumiko)
        {
            return false;
        }

        if (relationships.teruko
            < choice.requiredTeruko)
        {
            return false;
        }

        return true;
    }
}