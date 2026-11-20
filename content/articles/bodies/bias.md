# Bias: Why Intelligence Repeats Its Training

Ask an AI to write a short story about a CEO and a nurse having a meeting.

It will probably give you a CEO who is a he, and a nurse who is a she. It will probably make the CEO decisive and the nurse caring. It will do it politely, fluently, and without any sign that it made a choice at all.

That is what bias looks like in a large language model. Not a rant. Not an insult. Just a quiet default.

## Bias is not about intent. It is about repetition.

We tend to imagine bias as an opinion the model holds. Something we could argue with, or delete, or fix with a better instruction. Tell it to be neutral, and it will be neutral. That is not how it works.

A model does not have opinions. It has probabilities. It has learned from more written material than any one person could reasonably consume in a lifetime, and it has learned which words tend to follow which other words. When you ask for a CEO, it does not evaluate anything about CEOs. It tends to produce the pattern that appears most often in the material it learned from.

This is what makes AI bias difficult to spot. The model is not necessarily inventing anything. In many cases it is simply repeating patterns that already exist in the material it learned from. The uncomfortable part is that those patterns came from us.

The same mechanism is familiar in people. Psychologists have long described confirmation bias as the tendency to notice, remember, and weight information that confirms what we already believe (Nickerson, 1998). The model does not have beliefs, but the outcome can look surprisingly similar. Patterns that appear again and again gain influence simply because they appear again and again.

For a detailed treatment of how bias enters AI systems, see the National Institute of Standards and Technology's [Towards a Standard for Identifying and Managing Bias in Artificial Intelligence](https://www.nist.gov/publications/towards-standard-identifying-and-managing-bias-artificial-intelligence).

## Repetition becomes truth

This is uncomfortable because it removes a convenient excuse. If the model is biased, we cannot only blame the machine. We have to ask where the pattern came from. Who wrote the training data? Whose stories were published, quoted, and repeated until they became the statistical norm? Whose stories were missing?

Bias in this sense is not about one bad sentence. It is about what becomes invisible through repetition. The CEO who is a woman does not disappear because the model dislikes women. She becomes less likely because the pattern appears less often in the data the model learned from.

Absence is a form of bias too. When a perspective is rarely represented, the system does not need to actively exclude it. It only needs to do the statistically normal thing.

## Why "just be unbiased" does not work

That is also why "just be unbiased" does not work as a prompt. You are asking a statistical system to ignore statistics. It can try. It will often give you a more balanced answer if you ask explicitly. But the moment you stop watching, it will drift back to the default, because the default is what it learned.

The same is true for people. Telling someone to "be objective" does not remove their training. It only adds a new instruction on top of the old pattern. Without a check, a counter-example, or a second perspective, the old pattern wins.

This is the central claim of this series: intelligence does not fail mainly because it is stupid. It fails because it is optimized to repeat what worked before, and repetition feels like understanding.

## What to do instead

The practical question is not: How do we make the model perfectly neutral? We cannot. The practical question is: Whose perspective is missing from this answer, and does that absence change the decision I am about to make?

If you are writing fiction, it might not matter. If you are writing a job ad, a school example, or a summary of who counts as an expert, it matters a lot.

Try this next time you use AI:

- Ask for the default answer, then ask for the opposite default. Ask: Who else could this be?
- Ask: What would this look like from a perspective that is rarely represented in your training data?
- Notice how hard the model has to work for the second answer. That extra effort usually points to where the default sits.

A model will not tell you it is biased. It will usually present the answer as if it were obvious. The useful question is often not what the model included, but what it left out.

## Where this series goes next

Bias is Part 1 of Confidently Wrong. The next parts examine what happens when repetition meets other failure modes: sycophancy, where a system learns to agree rather than correct; overconfidence, where fluency is mistaken for knowledge; and hallucination, where a gap is filled with invention.

None of those are separate mysteries. They are what happens when a system that repeats its training is also rewarded for sounding plausible, agreeable, and sure.

### Sources

- [National Institute of Standards and Technology, Towards a Standard for Identifying and Managing Bias in Artificial Intelligence](https://www.nist.gov/publications/towards-standard-identifying-and-managing-bias-artificial-intelligence)
- [Nickerson, R. S. (1998). Confirmation bias: A ubiquitous phenomenon in many guises. _Review of General Psychology, 2_(2), 175–220.](https://doi.org/10.1037/1089-2680.2.2.175)
- [Anthropic, Towards Understanding Sycophancy in Language Models](https://arxiv.org/abs/2310.13548)
- [Kruger, J., & Dunning, D. (1999). Unskilled and unaware of it: How difficulties in recognizing one's own incompetence lead to inflated self-assessments. _Journal of Personality and Social Psychology, 77_(6), 1121–1134.](https://doi.org/10.1037/0022-3514.77.6.1121)
