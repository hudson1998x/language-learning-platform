// AUTO-GENERATED FILE. Do not edit directly.
// Run the component registry generator to regenerate.

import { register } from './registry'
import FrontendTheme from '@theme:frontend/Default'
import AdminTheme from '@theme:admin/Default'
export { FrontendTheme, AdminTheme } 

import { Homepage } from '@component/Pages/Homepage'
import { FlashCards } from '@component/Pages/FlashCards'
import { Study } from '@component/Pages/FlashCards/Study'
import { LanguageSelector } from '@component/LanguageSelector'
import { LlmNeededWall } from '@component/LlmNeededWall'
import { Text } from '@component/Text'
import { Spinner } from '@component/Spinner'

register('@component/Pages/Homepage', Homepage)
register('@component/Pages/FlashCards', FlashCards)
register('@component/Pages/FlashCards/Study', Study)
register('@component/LanguageSelector', LanguageSelector)
register('@component/LlmNeededWall', LlmNeededWall)
register('@component/Text', Text)
register('@component/Spinner', Spinner)

// auto imports
import '../../../../../.././App/Code/Local/Scenarios/Source/web/Pages/Scenarios/index.tsx';
import '../../../../../.././App/Code/Local/Scenarios/Source/web/Pages/ScenarioSession/index.tsx';
import '../../../../../.././App/Code/Community/LeMessage/Source/web/ChatPage/index.tsx';
import '../../../../../.././App/Code/Community/MusicTranslation/Source/web/TranslationPage/index.tsx';
import '../../../../../.././App/Code/Community/LLMProviders/ChatGPT/Source/web/configuration/model-selector/index.tsx';
import '../../../../../.././App/Code/Community/LLMProviders/ChatGPT/Source/web/help/connecting-chatgpt/index.tsx';
import '../../../../../.././App/Code/Community/LLMProviders/MistralChat/Source/web/configuration/model-selector/index.tsx';
import '../../../../../.././App/Code/Community/LLMProviders/MistralChat/Source/web/help/connecting-mistral/index.tsx';
import '../../../../../.././App/Code/Community/LLMProviders/Ollama/Source/web/configuration/model-selector/index.tsx';
import '../../../../../.././App/Code/Community/LLMFramework/Source/web/configuration/provider-selector/index.tsx';
import '../../../../../.././App/Code/Core/AppAdmin/Source/web/config-editor/index.tsx';
