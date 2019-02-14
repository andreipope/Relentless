# import unittest
# from appium import webdriver
# from altunityrunner import AltrunUnityDriver, AltElement
# import xmlrunner
# from base import CZBTests
# import time



# import sys
# reload(sys)
# sys.setdefaultencoding('utf8')

# class CZBArmyTests(CZBTests):
  
#     def setUp(self):
#         super(CZBArmyTests, self).setUp()
#         self.pass_authentification()
#         self.altdriver.wait_for_element('Button_Army').mobile_tap()


#     def test_check_all_cards(self):
#         army_cards=[]
       

#         gone_through_all_the_cards=False
#         while gone_through_all_the_cards==False:
#             cards_on_page=self.altdriver.find_elements('CreatureCard(Clone)/TitleText')
#             if cards_on_page==[]:
#                 cards_on_page=self.altdriver.find_elements('ItemCard(Clone)/TitleText')
#             for card in cards_on_page:
#                 card_name=card.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
#                 if self.check_name_is_in_list(army_cards,card_name):
#                     gone_through_all_the_cards=True
#                     break
#                 else:
#                     army_cards.append(card_name)

                   
#             self.altdriver.wait_for_element('ArmyPage(Clone)/Button_ArrowRight').mobile_tap()
#             time.sleep(1)
#         print(len(army_cards))
#         text_card_counter=self.altdriver.wait_for_element('CardsCounter/text').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')
#         # card_number=text_card_counter.split('/')
#         # self.assertEqual(len(army_cards),card_number[0])
#         self.assertEqual(len(army_cards),101)

# if __name__ == '__main__':
#    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))